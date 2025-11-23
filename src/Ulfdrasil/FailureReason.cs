namespace Ulfdrasil;

/// <summary>
/// Represents an error with a code, message, and optional details.
/// </summary>
public sealed record FailureReason
{
    /// <summary>
    /// Short code used to identify failure reason.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Descriptive explanation of failure reason.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Additional details for error.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailureReason"/> class.
    /// </summary>
    /// <param name="code">Short code used to identify failure reason.</param>
    /// <param name="description">Descriptive explanation of failure reason.</param>
    public FailureReason(string code, string description)
    {
        Code = code;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailureReason"/> class with details.
    /// </summary>
    /// <param name="code">Short code used to identify failure reason.</param>
    /// <param name="description">Descriptive explanation of failure reason.</param>
    /// <param name="details">Additional details for failure reason.</param>
    public FailureReason(string code, string description, IReadOnlyDictionary<string, string[]> details)
        : this(code, description)
    {
        Details = details;
    }
}