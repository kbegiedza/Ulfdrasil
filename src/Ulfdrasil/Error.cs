namespace Ulfdrasil;

/// <summary>
/// Represents an error with a code, message, and optional details.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// The standardized <see cref="ErrorCode"/>.
    /// </summary>
    public ErrorCode Code { get; init; }

    /// <summary>
    /// Descriptive message for error.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Additional details for error.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">Standardized <see cref="ErrorCode"/>.</param>
    /// <param name="message">Descriptive message for error.</param>
    public Error(ErrorCode code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class with details.
    /// </summary>
    /// <param name="code">Standardized <see cref="ErrorCode"/>.</param>
    /// <param name="message">Descriptive message for error.</param>
    /// <param name="details">Additional details for error.</param>
    public Error(ErrorCode code, string message, IReadOnlyDictionary<string, string[]> details)
        : this(code, message)
    {
        Details = details;
    }
}