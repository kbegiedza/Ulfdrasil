using Microsoft.Extensions.Logging;

using Ulfdrasil;
using Ulfdrasil.Hyperbatch.Abstractions;

namespace Ulfdrasil.Hyperbatch.Internal;

internal sealed class MicroBatchQueue<TKey, TRequest, TResponse>
{
    private readonly TKey _key;
    private readonly IHyperbatchBatchHandler<TKey, TRequest, TResponse> _handler;
    private readonly HyperbatchQueueOptions _options;
    private readonly HyperbatchRetryOptions _retryOptions;
    private readonly Func<HyperbatchRequest<TRequest>, int>? _tokenCounter;
    private readonly Func<HyperbatchQueueMetrics, bool>? _shouldFlush;
    private readonly Func<Exception, HyperbatchBatchFailure> _failureClassifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MicroBatchQueue<TKey, TRequest, TResponse>> _logger;
    private readonly Random _jitterRandom;
    private readonly List<HyperbatchWorkItem<TRequest, TResponse>> _pending = [];
    private readonly object _gate = new();
    private readonly SemaphoreSlim _processLock = new(1, 1);
    private CancellationTokenSource? _flushCts;
    private DateTimeOffset? _scheduledFlushAt;
    private DateTimeOffset? _oldestEnqueuedAt;
    private DateTimeOffset? _earliestDeadline;
    private int _queuedTokens;
    private bool _stopped;

    public MicroBatchQueue(
        TKey key,
        IHyperbatchBatchHandler<TKey, TRequest, TResponse> handler,
        HyperbatchQueueOptions options,
        HyperbatchRetryOptions retryOptions,
        Func<HyperbatchRequest<TRequest>, int>? tokenCounter,
        Func<HyperbatchQueueMetrics, bool>? shouldFlush,
        Func<Exception, HyperbatchBatchFailure> failureClassifier,
        TimeProvider timeProvider,
        ILogger<MicroBatchQueue<TKey, TRequest, TResponse>> logger,
        Random jitterRandom)
    {
        _key = key;
        _handler = handler;
        _options = options;
        _retryOptions = retryOptions;
        _tokenCounter = tokenCounter;
        _shouldFlush = shouldFlush;
        _failureClassifier = failureClassifier;
        _timeProvider = timeProvider;
        _logger = logger;
        _jitterRandom = jitterRandom;
    }

    public ValueTask<Result<TResponse>> EnqueueAsync(HyperbatchRequest<TRequest> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return new ValueTask<Result<TResponse>>(Task.FromCanceled<Result<TResponse>>(cancellationToken));
        }

        HyperbatchWorkItem<TRequest, TResponse>? item;
        List<HyperbatchWorkItem<TRequest, TResponse>>? batchToFlushBeforeAdd = null;
        List<HyperbatchWorkItem<TRequest, TResponse>>? batchToFlushAfterAdd = null;
        DateTimeOffset? scheduleAt = null;
        string? flushReason = null;

        lock (_gate)
        {
            if (_stopped)
            {
                throw new ObjectDisposedException(nameof(MicroBatchQueue<TKey, TRequest, TResponse>));
            }

            PruneCompletedItemsNoLock();

            if (_options.MaxQueueSize > 0
                && _pending.Count >= _options.MaxQueueSize
                && _options.QueueFullMode == HyperbatchQueueFullMode.Reject)
            {
                item = new HyperbatchWorkItem<TRequest, TResponse>(
                    request,
                    0,
                    _timeProvider.GetUtcNow(),
                    cancellationToken);
                _logger.LogWarning(
                    "Queue full for key {Key}. Rejecting request. QueueLength={QueueLength}, MaxQueueSize={MaxQueueSize}.",
                    _key,
                    _pending.Count,
                    _options.MaxQueueSize);
                item.TrySetResult(Result.Failure<TResponse>(HyperbatchProblems.QueueFull()));
                return new ValueTask<Result<TResponse>>(item.Completion.Task);
            }

            var tokenCount = GetTokenCount(request);

            if (_options.MaxBatchTokens > 0
                && _pending.Count > 0
                && _queuedTokens + tokenCount > _options.MaxBatchTokens)
            {
                batchToFlushBeforeAdd = DequeueAllNoLock();
            }

            item = new HyperbatchWorkItem<TRequest, TResponse>(
                request,
                tokenCount,
                _timeProvider.GetUtcNow(),
                cancellationToken);

            _pending.Add(item);
            _queuedTokens += tokenCount;
            _oldestEnqueuedAt ??= item.EnqueuedAt;
            if (item.Deadline is not null)
            {
                _earliestDeadline = _earliestDeadline is null
                    ? item.Deadline
                    : Min(_earliestDeadline.Value, item.Deadline.Value);
            }

            var metrics = new HyperbatchQueueMetrics(_pending.Count, _queuedTokens, _oldestEnqueuedAt, _earliestDeadline);
            if (ShouldFlushNoLock(metrics, out flushReason))
            {
                batchToFlushAfterAdd = DequeueAllNoLock();
            }
            else
            {
                scheduleAt = CalculateNextFlushAt(metrics);
            }
        }

        if (batchToFlushBeforeAdd is not null)
        {
            _logger.LogDebug(
                "Flushing batch for key {Key} before add due to token budget. BatchSize={BatchSize}, Tokens={TokenCount}.",
                _key,
                batchToFlushBeforeAdd.Count,
                batchToFlushBeforeAdd.Sum(item => item.TokenCount));
            _ = ProcessBatchAsync(batchToFlushBeforeAdd, CancellationToken.None);
        }

        if (batchToFlushAfterAdd is not null)
        {
            _logger.LogDebug(
                "Flushing batch for key {Key}. Reason={Reason}, BatchSize={BatchSize}, Tokens={TokenCount}.",
                _key,
                flushReason ?? "Unknown",
                batchToFlushAfterAdd.Count,
                batchToFlushAfterAdd.Sum(item => item.TokenCount));
            _ = ProcessBatchAsync(batchToFlushAfterAdd, CancellationToken.None);
        }
        else if (scheduleAt.HasValue)
        {
            ScheduleFlush(scheduleAt.Value);
        }

        return new ValueTask<Result<TResponse>>(item.Completion.Task);
    }

    public async Task StopAsync()
    {
        List<HyperbatchWorkItem<TRequest, TResponse>>? batch;
        lock (_gate)
        {
            if (_stopped)
            {
                return;
            }

            _stopped = true;
            batch = DequeueAllNoLock();
        }

        if (batch is not null && batch.Count > 0)
        {
            _logger.LogInformation(
                "Stopping queue for key {Key}. Flushing remaining {BatchSize} items.",
                _key,
                batch.Count);
            await ProcessBatchAsync(batch, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private bool ShouldFlushNoLock(HyperbatchQueueMetrics metrics, out string? reason)
    {
        if (_options.MaxBatchSize > 0 && metrics.QueueLength >= _options.MaxBatchSize)
        {
            reason = "MaxBatchSize";
            return true;
        }

        if (_options.MaxBatchTokens > 0 && metrics.QueuedTokens >= _options.MaxBatchTokens)
        {
            reason = "MaxBatchTokens";
            return true;
        }

        if (_options.MaxQueueSize > 0
            && metrics.QueueLength >= _options.MaxQueueSize
            && _options.QueueFullMode == HyperbatchQueueFullMode.Flush)
        {
            reason = "MaxQueueSize";
            return true;
        }

        if (_shouldFlush?.Invoke(metrics) == true)
        {
            reason = "Backpressure";
            return true;
        }

        reason = null;
        return false;
    }

    private DateTimeOffset? CalculateNextFlushAt(HyperbatchQueueMetrics metrics)
    {
        if (metrics.QueueLength == 0)
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        var waitUntil = _options.MaxWaitTime <= TimeSpan.Zero
            ? now
            : metrics.OldestEnqueuedAt?.Add(_options.MaxWaitTime);

        if (metrics.EarliestDeadline is not null && waitUntil is not null)
        {
            return Min(waitUntil.Value, metrics.EarliestDeadline.Value);
        }

        return waitUntil ?? metrics.EarliestDeadline;
    }

    private void ScheduleFlush(DateTimeOffset flushAt)
    {
        CancellationToken token;
        TimeSpan delay;

        lock (_gate)
        {
            if (_stopped)
            {
                return;
            }

            if (_scheduledFlushAt is not null && _scheduledFlushAt <= flushAt)
            {
                return;
            }

            _scheduledFlushAt = flushAt;
            _flushCts?.Cancel();
            _flushCts?.Dispose();
            _flushCts = new CancellationTokenSource();
            token = _flushCts.Token;
            var now = _timeProvider.GetUtcNow();
            delay = flushAt <= now ? TimeSpan.Zero : flushAt - now;
        }

        _logger.LogDebug(
            "Scheduling flush for key {Key} in {DelayMs}ms at {FlushAt}.",
            _key,
            delay.TotalMilliseconds,
            flushAt);
        _ = RunFlushTimerAsync(delay, token);
    }

    private async Task RunFlushTimerAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _logger.LogDebug("Flush timer fired for key {Key}.", _key);
        await FlushAsync().ConfigureAwait(false);
    }

    private async Task FlushAsync()
    {
        List<HyperbatchWorkItem<TRequest, TResponse>>? batch;
        lock (_gate)
        {
            batch = _pending.Count == 0 ? null : DequeueAllNoLock();
        }

        if (batch is not null && batch.Count > 0)
        {
            _logger.LogDebug(
                "Timer flush for key {Key}. BatchSize={BatchSize}.",
                _key,
                batch.Count);
            await ProcessBatchAsync(batch, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task ProcessBatchAsync(
        List<HyperbatchWorkItem<TRequest, TResponse>> batch,
        CancellationToken cancellationToken)
    {
        await _processLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var activeItems = batch.Where(item => !item.CancellationToken.IsCancellationRequested).ToList();
            foreach (var item in batch.Where(item => item.CancellationToken.IsCancellationRequested))
            {
                item.TrySetCanceled();
            }

            if (activeItems.Count == 0)
            {
                return;
            }

            _logger.LogDebug(
                "Processing batch for key {Key}. BatchSize={BatchSize}.",
                _key,
                activeItems.Count);
            var results = await ExecuteWithPoliciesAsync(activeItems, cancellationToken).ConfigureAwait(false);
            for (var index = 0; index < activeItems.Count; index++)
            {
                activeItems[index].TrySetResult(results[index]);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            foreach (var item in batch)
            {
                item.TrySetCanceled();
            }
        }
        catch (Exception exception)
        {
            var failure = _failureClassifier(exception);
            _logger.LogWarning(
                exception,
                "Batch processing failed for key {Key}. FailureKind={FailureKind}.",
                _key,
                failure.Kind);
            var failureResult = Result.Failure<TResponse>(failure.Problem);
            foreach (var item in batch)
            {
                item.TrySetResult(failureResult);
            }
        }
        finally
        {
            _processLock.Release();
        }
    }

    private async Task<IReadOnlyList<Result<TResponse>>> ExecuteWithPoliciesAsync(
        IReadOnlyList<HyperbatchWorkItem<TRequest, TResponse>> items,
        CancellationToken cancellationToken)
    {
        var attempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var requests = items.Select(item => item.Request).ToList();
                var results = await _handler.HandleBatchAsync(_key, requests, cancellationToken).ConfigureAwait(false);
                if (results.Count != items.Count)
                {
                    _logger.LogError(
                        "Invalid result count for key {Key}. Expected {Expected} but got {Actual}.",
                        _key,
                        items.Count,
                        results.Count);
                    return FailAll(items.Count, HyperbatchProblems.InvalidResultCount(items.Count, results.Count));
                }

                return results;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var failure = _failureClassifier(exception);

                if (failure.SupportsBisect && items.Count > 1)
                {
                    _logger.LogWarning(
                        "Bisecting batch for key {Key} due to batch failure.",
                        _key);
                    var splitIndex = items.Count / 2;
                    var left = items.Take(splitIndex).ToList();
                    var right = items.Skip(splitIndex).ToList();
                    var leftResults = await ExecuteWithPoliciesAsync(left, cancellationToken).ConfigureAwait(false);
                    var rightResults = await ExecuteWithPoliciesAsync(right, cancellationToken).ConfigureAwait(false);
                    return leftResults.Concat(rightResults).ToList();
                }

                if (failure.IsRetryable && attempts + 1 < _retryOptions.MaxAttempts)
                {
                    var delay = GetRetryDelay(attempts);
                    attempts++;
                    var nextAttempt = attempts + 1;
                    _logger.LogWarning(
                        "Retrying batch for key {Key}. Attempt {Attempt} of {MaxAttempts}. DelayMs={DelayMs}.",
                        _key,
                        nextAttempt,
                        _retryOptions.MaxAttempts,
                        delay.TotalMilliseconds);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }

                    continue;
                }

                _logger.LogWarning(
                    "Batch failed for key {Key} after {Attempts} attempts. FailureKind={FailureKind}.",
                    _key,
                    attempts + 1,
                    failure.Kind);
                return FailAll(items.Count, failure.Problem);
            }
        }
    }

    private static IReadOnlyList<Result<TResponse>> FailAll(int count, Problem problem)
    {
        var results = new Result<TResponse>[count];
        for (var index = 0; index < count; index++)
        {
            results[index] = Result.Failure<TResponse>(problem);
        }

        return results;
    }

    private int GetTokenCount(HyperbatchRequest<TRequest> request)
    {
        if (request.TokenCount.HasValue)
        {
            return ValidateTokenCount(request.TokenCount.Value);
        }

        if (_tokenCounter is not null)
        {
            return ValidateTokenCount(_tokenCounter(request));
        }

        if (_options.MaxBatchTokens > 0)
        {
            throw new InvalidOperationException("Token count is required when MaxBatchTokens is configured.");
        }

        return 0;
    }

    private static int ValidateTokenCount(int tokenCount)
    {
        if (tokenCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenCount), "Token count must be non-negative.");
        }

        return tokenCount;
    }

    private void PruneCompletedItemsNoLock()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        var removed = _pending.RemoveAll(item => item.Completion.Task.IsCompleted);
        if (removed > 0)
        {
            RecalculateMetricsNoLock();
        }
    }

    private void RecalculateMetricsNoLock()
    {
        _queuedTokens = 0;
        _oldestEnqueuedAt = null;
        _earliestDeadline = null;

        foreach (var item in _pending)
        {
            _queuedTokens += item.TokenCount;
            _oldestEnqueuedAt ??= item.EnqueuedAt;
            if (item.Deadline is not null)
            {
                _earliestDeadline = _earliestDeadline is null
                    ? item.Deadline
                    : Min(_earliestDeadline.Value, item.Deadline.Value);
            }
        }
    }

    private List<HyperbatchWorkItem<TRequest, TResponse>> DequeueAllNoLock()
    {
        var batch = new List<HyperbatchWorkItem<TRequest, TResponse>>(_pending);
        _pending.Clear();
        _queuedTokens = 0;
        _oldestEnqueuedAt = null;
        _earliestDeadline = null;
        _scheduledFlushAt = null;
        _flushCts?.Cancel();
        _flushCts?.Dispose();
        _flushCts = null;
        return batch;
    }

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right)
        => left <= right ? left : right;

    private TimeSpan GetRetryDelay(int attempt)
    {
        if (_retryOptions.MaxAttempts <= 1)
        {
            return TimeSpan.Zero;
        }

        var exponential = _retryOptions.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var capped = Math.Min(exponential, _retryOptions.MaxDelay.TotalMilliseconds);
        var jitter = _retryOptions.JitterFactor <= 0
            ? 0
            : capped * _retryOptions.JitterFactor * (_jitterRandom.NextDouble() - 0.5) * 2;
        var delayMs = Math.Max(0, capped + jitter);
        return TimeSpan.FromMilliseconds(delayMs);
    }
}
