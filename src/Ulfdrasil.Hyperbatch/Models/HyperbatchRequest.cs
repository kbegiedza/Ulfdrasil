namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Represents a request with optional metadata for batching decisions.
/// </summary>
/// <typeparam name="TRequest">The request payload type.</typeparam>
public sealed record HyperbatchRequest<TRequest>
{
    /// <summary>
    /// Gets the request payload.
    /// </summary>
    public required TRequest Request { get; init; }

    /// <summary>
    /// Gets the optional deadline for this request.
    /// </summary>
    public DateTimeOffset? Deadline { get; init; }

    /// <summary>
    /// Gets the optional token count for this request.
    /// </summary>
    public int? TokenCount { get; init; }

    /// <summary>
    /// Creates a request from the provided payload.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <returns>A new <see cref="HyperbatchRequest{TRequest}"/>.</returns>
    public static HyperbatchRequest<TRequest> From(TRequest request) => new() { Request = request };
}
