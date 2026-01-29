using Ulfdrasil;

namespace Ulfdrasil.Hyperbatch.Abstractions;

/// <summary>
/// Defines a handler that processes a batch of requests for a given compatibility key.
/// </summary>
/// <typeparam name="TKey">The key that defines batching compatibility.</typeparam>
/// <typeparam name="TRequest">The request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public interface IHyperbatchBatchHandler<TKey, TRequest, TResponse>
{
    /// <summary>
    /// Handles a batch of requests and returns a result per request in the same order.
    /// </summary>
    /// <param name="key">The compatibility key for the batch.</param>
    /// <param name="requests">The requests to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result for each request in the same order.</returns>
    Task<IReadOnlyList<Result<TResponse>>> HandleBatchAsync(
        TKey key,
        IReadOnlyList<HyperbatchRequest<TRequest>> requests,
        CancellationToken cancellationToken = default);
}
