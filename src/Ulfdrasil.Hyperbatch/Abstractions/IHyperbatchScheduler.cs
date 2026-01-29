using Ulfdrasil;

namespace Ulfdrasil.Hyperbatch.Abstractions;

/// <summary>
/// Defines a scheduler that batches requests by compatibility key.
/// </summary>
/// <typeparam name="TKey">The key that defines batching compatibility.</typeparam>
/// <typeparam name="TRequest">The request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public interface IHyperbatchScheduler<TKey, TRequest, TResponse>
{
    /// <summary>
    /// Enqueues a request for batching and returns the per-item result.
    /// </summary>
    /// <param name="key">The compatibility key.</param>
    /// <param name="request">The request to enqueue.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result for the request.</returns>
    ValueTask<Result<TResponse>> EnqueueAsync(
        TKey key,
        HyperbatchRequest<TRequest> request,
        CancellationToken cancellationToken = default);
}
