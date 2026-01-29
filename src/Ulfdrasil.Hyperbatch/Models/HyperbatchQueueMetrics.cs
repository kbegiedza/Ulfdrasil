namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Provides metrics about the current queue state for backpressure decisions.
/// </summary>
/// <param name="QueueLength">The number of items currently queued.</param>
/// <param name="QueuedTokens">The total token count currently queued.</param>
/// <param name="OldestEnqueuedAt">The enqueue time of the oldest item.</param>
/// <param name="EarliestDeadline">The earliest deadline among queued items.</param>
public sealed record HyperbatchQueueMetrics(
    int QueueLength,
    int QueuedTokens,
    DateTimeOffset? OldestEnqueuedAt,
    DateTimeOffset? EarliestDeadline);
