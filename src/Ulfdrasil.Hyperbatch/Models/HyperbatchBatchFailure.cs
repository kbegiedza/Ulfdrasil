using Ulfdrasil;

namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Represents a classified batch-level failure.
/// </summary>
/// <param name="Problem">The problem describing the failure.</param>
/// <param name="Kind">The classification of the failure.</param>
/// <param name="IsRetryable">Whether the failure should be retried.</param>
/// <param name="SupportsBisect">Whether the failure supports bisecting.</param>
public sealed record HyperbatchBatchFailure(
    Problem Problem,
    HyperbatchFailureKind Kind,
    bool IsRetryable,
    bool SupportsBisect)
{
    /// <summary>
    /// Creates a client-side failure.
    /// </summary>
    public static HyperbatchBatchFailure Client(Problem problem, bool supportsBisect = false)
        => new(problem, HyperbatchFailureKind.Client, false, supportsBisect);

    /// <summary>
    /// Creates a transient failure.
    /// </summary>
    public static HyperbatchBatchFailure Transient(Problem problem, bool isRetryable = true)
        => new(problem, HyperbatchFailureKind.Transient, isRetryable, false);

    /// <summary>
    /// Creates a transport failure.
    /// </summary>
    public static HyperbatchBatchFailure Transport(Problem problem, bool isRetryable = true)
        => new(problem, HyperbatchFailureKind.Transport, isRetryable, false);

    /// <summary>
    /// Creates an unknown failure.
    /// </summary>
    public static HyperbatchBatchFailure Unknown(Problem problem)
        => new(problem, HyperbatchFailureKind.Unknown, false, false);
}
