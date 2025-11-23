namespace Ulfdrasil;

/// <summary>
/// Represents a description of a failure that occurred while
/// processing a request or executing an operation.
/// </summary>
public sealed record Problem
{
    /// <summary>
    /// Short code that uniquely identifies the type of problem.
    /// This is intended to be stable and suitable for programmatic handling
    /// (for example, <c>"validation_error"</c> or <c>"conflict.resource_already_exists"</c>).
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Description of the failure, suitable for logs or displaying to users.
    /// This should be concise but descriptive enough to understand what went wrong.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Optional, structured details that provide additional context about the failure.
    /// Keys are typically field or parameter names and values are explanations,
    /// but the exact semantics are application-specific.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Details { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class with the
    /// specified problem <paramref name="code"/> and <paramref name="description"/>.
    /// </summary>
    /// <param name="code">
    /// Short code that uniquely identifies the type of problem.
    /// </param>
    /// <param name="description">
    /// Description of the failure.
    /// </param>
    public Problem(string code, string description)
    {
        Code = code;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class with the
    /// specified problem <paramref name="code"/>, <paramref name="description"/>
    /// and additional <paramref name="details"/>.
    /// </summary>
    /// <param name="code">
    /// Short code that uniquely identifies the type of problem.
    /// </param>
    /// <param name="description">
    /// Description of the failure.
    /// </param>
    /// <param name="details">
    /// Optional, structured details that provide additional context about the
    /// failure.
    /// </param>
    public Problem(string code, string description, IReadOnlyDictionary<string, string> details)
        : this(code, description)
    {
        Details = details;
    }
}