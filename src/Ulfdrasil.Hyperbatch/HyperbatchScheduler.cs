using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Ulfdrasil;
using Ulfdrasil.Hyperbatch.Abstractions;
using Ulfdrasil.Hyperbatch.Internal;

namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Provides a scheduler that batches requests by compatibility key.
/// </summary>
/// <typeparam name="TKey">The key that defines batching compatibility.</typeparam>
/// <typeparam name="TRequest">The request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public sealed class HyperbatchScheduler<TKey, TRequest, TResponse> :
    IHyperbatchScheduler<TKey, TRequest, TResponse>,
    IAsyncDisposable
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, MicroBatchQueue<TKey, TRequest, TResponse>> _queues;
    private readonly IHyperbatchBatchHandler<TKey, TRequest, TResponse> _handler;
    private readonly HyperbatchSchedulerOptions<TKey, TRequest, TResponse> _options;
    private readonly Func<Exception, HyperbatchBatchFailure> _failureClassifier;
    private readonly ILogger<HyperbatchScheduler<TKey, TRequest, TResponse>> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HyperbatchScheduler{TKey, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="handler">The batch handler.</param>
    /// <param name="options">The scheduler options.</param>
    public HyperbatchScheduler(
        IHyperbatchBatchHandler<TKey, TRequest, TResponse> handler,
        HyperbatchSchedulerOptions<TKey, TRequest, TResponse>? options = null)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _options = options ?? new HyperbatchSchedulerOptions<TKey, TRequest, TResponse>();
        _failureClassifier = _options.FailureClassifier ?? DefaultFailureClassifier;
        _logger = _options.LoggerFactory?.CreateLogger<HyperbatchScheduler<TKey, TRequest, TResponse>>()
                  ?? NullLogger<HyperbatchScheduler<TKey, TRequest, TResponse>>.Instance;
        _queues = new ConcurrentDictionary<TKey, MicroBatchQueue<TKey, TRequest, TResponse>>(_options.KeyComparer);
    }

    /// <inheritdoc />
    public ValueTask<Result<TResponse>> EnqueueAsync(
        TKey key,
        HyperbatchRequest<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            _logger.LogWarning("Attempted to enqueue on disposed scheduler.");
            throw new ObjectDisposedException(nameof(HyperbatchScheduler<TKey, TRequest, TResponse>));
        }

        ArgumentNullException.ThrowIfNull(request);

        var queue = _queues.GetOrAdd(key, CreateQueue);
        _logger.LogDebug("Enqueue request for key {Key}.", key);
        return queue.EnqueueAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation("Disposing scheduler and draining queues.");
        _disposed = true;
        var queues = _queues.Values.ToArray();
        _queues.Clear();

        if (queues.Length == 0)
        {
            _logger.LogDebug("No queues to drain.");
            return;
        }

        var stopTasks = queues.Select(queue => queue.StopAsync()).ToArray();

        await Task.WhenAll(stopTasks).ConfigureAwait(false);
        _logger.LogInformation("Scheduler disposed.");
    }

    private MicroBatchQueue<TKey, TRequest, TResponse> CreateQueue(TKey key)
    {
        var options = _options.QueueOptionsProvider?.Invoke(key) ?? _options.DefaultQueueOptions;

        _logger.LogInformation(
            "Creating micro-batch queue for key {Key} with MaxBatchSize={MaxBatchSize}, MaxBatchTokens={MaxBatchTokens}, MaxQueueSize={MaxQueueSize}, MaxWaitTime={MaxWaitTime}, QueueFullMode={QueueFullMode}.",
            key,
            options.MaxBatchSize,
            options.MaxBatchTokens,
            options.MaxQueueSize,
            options.MaxWaitTime,
            options.QueueFullMode);

        return new MicroBatchQueue<TKey, TRequest, TResponse>(
            key,
            _handler,
            options,
            _options.RetryOptions,
            _options.TokenCounter,
            _options.ShouldFlush,
            _failureClassifier,
            _options.TimeProvider,
            _options.LoggerFactory?.CreateLogger<MicroBatchQueue<TKey, TRequest, TResponse>>()
            ?? NullLogger<MicroBatchQueue<TKey, TRequest, TResponse>>.Instance,
            Random.Shared);
    }

    private static HyperbatchBatchFailure DefaultFailureClassifier(Exception exception)
    {
        if (exception is HyperbatchBatchException batchException)
        {
            return batchException.Failure;
        }

        return HyperbatchBatchFailure.Transport(HyperbatchProblems.BatchFailed(exception.Message));
    }
}
