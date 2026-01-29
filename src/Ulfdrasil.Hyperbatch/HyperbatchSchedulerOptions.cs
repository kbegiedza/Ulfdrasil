using Microsoft.Extensions.Logging;

namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Configures scheduler behavior and batching policies.
/// </summary>
/// <typeparam name="TKey">The key that defines batching compatibility.</typeparam>
/// <typeparam name="TRequest">The request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public sealed record HyperbatchSchedulerOptions<TKey, TRequest, TResponse>
{
    /// <summary>
    /// Gets the default queue options when no key-specific override is provided.
    /// </summary>
    public HyperbatchQueueOptions DefaultQueueOptions { get; init; } = new();

    /// <summary>
    /// Gets the per-key queue options provider.
    /// </summary>
    public Func<TKey, HyperbatchQueueOptions>? QueueOptionsProvider { get; init; }

    /// <summary>
    /// Gets the token counter used for token-based batching.
    /// </summary>
    public Func<HyperbatchRequest<TRequest>, int>? TokenCounter { get; init; }

    /// <summary>
    /// Gets the backpressure predicate that can trigger an early flush.
    /// </summary>
    public Func<HyperbatchQueueMetrics, bool>? ShouldFlush { get; init; }

    /// <summary>
    /// Gets the retry options used for batch-level failures.
    /// </summary>
    public HyperbatchRetryOptions RetryOptions { get; init; } = HyperbatchRetryOptions.None;

    /// <summary>
    /// Gets the failure classifier for exceptions thrown by the handler.
    /// </summary>
    public Func<Exception, HyperbatchBatchFailure>? FailureClassifier { get; init; }

    /// <summary>
    /// Gets the key comparer used to partition queues.
    /// </summary>
    public IEqualityComparer<TKey> KeyComparer { get; init; } = EqualityComparer<TKey>.Default;

    /// <summary>
    /// Gets the time provider used for scheduling and deadlines.
    /// </summary>
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;

    /// <summary>
    /// Gets the logger factory used to create scheduler and queue loggers.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; init; }
}
