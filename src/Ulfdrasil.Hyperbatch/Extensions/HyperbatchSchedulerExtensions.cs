using Ulfdrasil;
using Ulfdrasil.Hyperbatch.Abstractions;

namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Provides convenience extensions for the scheduler.
/// </summary>
public static class HyperbatchSchedulerExtensions
{
    /// <summary>
    /// Enqueues a request payload for batching.
    /// </summary>
    /// <typeparam name="TKey">The key that defines batching compatibility.</typeparam>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="scheduler">The scheduler instance.</param>
    /// <param name="key">The compatibility key.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result for the request.</returns>
    public static ValueTask<Result<TResponse>> EnqueueAsync<TKey, TRequest, TResponse>(
        this IHyperbatchScheduler<TKey, TRequest, TResponse> scheduler,
        TKey key,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        return scheduler.EnqueueAsync(key, HyperbatchRequest<TRequest>.From(request), cancellationToken);
    }
}
