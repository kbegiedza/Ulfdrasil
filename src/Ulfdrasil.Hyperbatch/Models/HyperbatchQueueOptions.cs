namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Defines queue-level batching limits and latency budget.
/// </summary>
public sealed record HyperbatchQueueOptions
{
    /// <summary>
    /// Gets the maximum number of items per batch. Set to 0 to disable size-based flushing.
    /// </summary>
    public int MaxBatchSize { get; init; }

    /// <summary>
    /// Gets the maximum token budget per batch. Set to 0 to disable token-based flushing.
    /// </summary>
    public int MaxBatchTokens { get; init; }

    /// <summary>
    /// Gets the maximum number of enqueued items before backpressure triggers. Set to 0 to disable.
    /// </summary>
    public int MaxQueueSize { get; init; }

    /// <summary>
    /// Gets the maximum time to wait before flushing a batch.
    /// </summary>
    public TimeSpan MaxWaitTime { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Gets the behavior when the queue is full.
    /// </summary>
    public HyperbatchQueueFullMode QueueFullMode { get; init; } = HyperbatchQueueFullMode.Flush;
}
