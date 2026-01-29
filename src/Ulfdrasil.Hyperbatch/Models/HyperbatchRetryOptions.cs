namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Defines retry behavior for batch-level failures.
/// </summary>
public sealed record HyperbatchRetryOptions
{
    /// <summary>
    /// Gets a retry configuration that performs no retries.
    /// </summary>
    public static HyperbatchRetryOptions None { get; } = new() { MaxAttempts = 1 };

    /// <summary>
    /// Gets the maximum number of attempts including the first attempt.
    /// </summary>
    public int MaxAttempts { get; init; } = 1;

    /// <summary>
    /// Gets the base delay for exponential backoff.
    /// </summary>
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets the maximum delay between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets the jitter factor applied to retry delays.
    /// </summary>
    public double JitterFactor { get; init; } = 0.2;
}
