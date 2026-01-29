namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Defines how to handle enqueues when the queue is at capacity.
/// </summary>
public enum HyperbatchQueueFullMode
{
    /// <summary>
    /// Flush the current queue to make room.
    /// </summary>
    Flush,

    /// <summary>
    /// Reject the new request with a failure result.
    /// </summary>
    Reject
}
