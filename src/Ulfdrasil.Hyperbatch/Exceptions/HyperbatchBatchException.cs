namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Represents a batch-level failure with classification metadata.
/// </summary>
public sealed class HyperbatchBatchException : Exception
{
    /// <summary>
    /// Gets the failure classification.
    /// </summary>
    public HyperbatchBatchFailure Failure { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HyperbatchBatchException"/> class.
    /// </summary>
    /// <param name="failure">The failure classification.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    public HyperbatchBatchException(HyperbatchBatchFailure failure, Exception? innerException = null)
        : base(failure.Problem.Description, innerException)
    {
        Failure = failure;
    }
}
