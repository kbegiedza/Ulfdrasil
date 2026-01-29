namespace Exsaga.Console.EmbeddingsLoad;

/// <summary>
/// Represents a single embedding request payload for the hyperbatching example.
/// </summary>
public sealed record EmbeddingRequest
{
    /// <summary>
    /// Gets the input text to embed.
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// Gets the model name.
    /// </summary>
    public string Model { get; init; } = "text-embedding-3-small";

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Gets the optional output vector dimensions.
    /// </summary>
    public int? Dimensions { get; init; }
}
