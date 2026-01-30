using Microsoft.Extensions.Logging;

using Ulfdrasil;
using Ulfdrasil.Hyperbatch;
using Ulfdrasil.Hyperbatch.Abstractions;
using Exsaga.Hyperbatching.Models;

namespace Exsaga.Hyperbatching.Services;

/// <summary>
/// Example batch worker that simulates embedding generation.
/// </summary>
public sealed class EmbeddingsBatchWorker : IHyperbatchBatchHandler<BatchKey, EmbeddingRequest, EmbeddingResponse>
{
    private readonly ILogger<EmbeddingsBatchWorker> _logger;
    private readonly TimeProvider _timeProvider;

    public EmbeddingsBatchWorker(ILogger<EmbeddingsBatchWorker> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<Result<EmbeddingResponse>>> HandleBatchAsync(
        BatchKey key,
        IReadOnlyList<HyperbatchRequest<EmbeddingRequest>> requests,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing embeddings batch for {Provider}/{Operation}/{Model} Tenant={Tenant} BatchSize={BatchSize}.",
            key.Provider,
            key.Operation,
            key.Model,
            key.Tenant,
            requests.Count);

        if (requests.Count == 0)
        {
            return Array.Empty<Result<EmbeddingResponse>>();
        }

        if (requests.Any(request => request.Request.Input.Any(i => i.Equals("RATE_LIMIT", StringComparison.OrdinalIgnoreCase))))
        {
            throw new HyperbatchBatchException(
                HyperbatchBatchFailure.Transient(
                    HyperbatchProblems.BatchFailed("Upstream rate limited.")));
        }

        if (requests.Count > 1
            && requests.Any(request => request.Request.Input.Any(i => i.Equals("INVALID", StringComparison.OrdinalIgnoreCase))))
        {
            throw new HyperbatchBatchException(
                HyperbatchBatchFailure.Client(
                    HyperbatchProblems.ValidationFailed("Invalid input in batch."),
                    supportsBisect: true));
        }

        await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);

        var results = new List<Result<EmbeddingResponse>>(requests.Count);
        foreach (var request in requests)
        {
            results.Add(CreateResult(request));
        }

        return results;
    }

    private Result<EmbeddingResponse> CreateResult(HyperbatchRequest<EmbeddingRequest> request)
    {
        var input = request.Request.Input;
        if (input == null || input.Length == 0 || input.All(string.IsNullOrWhiteSpace)
            || input.Any(i => i.Equals("INVALID", StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<EmbeddingResponse>(
                HyperbatchProblems.ValidationFailed("Input must be non-empty and valid."));
        }

        var model = request.Request.Model;
        var tokenCount = request.TokenCount ?? TokenEstimator.Estimate(input);
        var dimensions = request.Request.Dimensions ?? 8;
        var vector = CreateVector(dimensions, tokenCount);

        return Result.Success(
            new EmbeddingResponse
            {
                Model = model,
                Vector = vector,
                TokenCount = tokenCount
            });
    }

    private float[] CreateVector(int dimensions, int tokenCount)
    {
        if (dimensions <= 0)
        {
            dimensions = 8;
        }

        var vector = new float[dimensions];
        var random = new Random(tokenCount ^ dimensions ^ _timeProvider.GetUtcNow().Millisecond);

        for (var index = 0; index < dimensions; index++)
        {
            vector[index] = (float)(random.NextDouble() * 2 - 1);
        }

        return vector;
    }
}
