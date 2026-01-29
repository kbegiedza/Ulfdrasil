using Ulfdrasil;

namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Defines common problems used by Hyperbatch.
/// </summary>
public static class HyperbatchProblems
{
    /// <summary>
    /// Gets a problem describing a full queue.
    /// </summary>
    public static Problem QueueFull() => new("hyperbatch.queue_full", "Hyperbatch queue is full.");

    /// <summary>
    /// Gets a problem describing an invalid result count.
    /// </summary>
    public static Problem InvalidResultCount(int expected, int actual) =>
        new("hyperbatch.invalid_result_count",
            $"Batch handler returned {actual} results for {expected} requests.");

    /// <summary>
    /// Gets a problem describing a batch failure.
    /// </summary>
    public static Problem BatchFailed(string description) => new("hyperbatch.batch_failed", description);

    /// <summary>
    /// Gets a problem describing a validation failure.
    /// </summary>
    public static Problem ValidationFailed(string description) => new("hyperbatch.validation_failed", description);
}
