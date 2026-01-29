namespace Exsaga.Hyperbatching.Models;

/// <summary>
/// Represents a single embedding response.
/// </summary>
public sealed record EmbeddingResponse
{
    /// <summary>
    /// Gets the model name.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Gets the embedding vector.
    /// </summary>
    public required float[] Vector { get; init; }

    /// <summary>
    /// Gets the token count used for the embedding.
    /// </summary>
    public int TokenCount { get; init; }
}
