namespace Exsaga.Hyperbatching.Models;

/// <summary>
/// Defines the compatibility key for batching.
/// </summary>
/// <param name="Provider">The upstream provider.</param>
/// <param name="Operation">The operation name.</param>
/// <param name="Model">The model identifier.</param>
/// <param name="Tenant">The tenant bucket.</param>
public sealed record BatchKey(string Provider, string Operation, string Model, string Tenant);
