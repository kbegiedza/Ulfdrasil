using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Ulfdrasil.Hyperbatch;
using Ulfdrasil.Hyperbatch.Abstractions;
using Exsaga.Hyperbatching.Models;
using Exsaga.Hyperbatching.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<IHyperbatchBatchHandler<BatchKey, EmbeddingRequest, EmbeddingResponse>, EmbeddingsBatchWorker>();
builder.Services.AddSingleton<IHyperbatchScheduler<BatchKey, EmbeddingRequest, EmbeddingResponse>>(sp =>
{
    var handler = sp.GetRequiredService<IHyperbatchBatchHandler<BatchKey, EmbeddingRequest, EmbeddingResponse>>();
    var timeProvider = sp.GetRequiredService<TimeProvider>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var options = new HyperbatchSchedulerOptions<BatchKey, EmbeddingRequest, EmbeddingResponse>
    {
        LoggerFactory = loggerFactory,
        TimeProvider = timeProvider,
        DefaultQueueOptions = new HyperbatchQueueOptions
        {
            MaxBatchSize = 8,
            MaxBatchTokens = 8192,
            MaxQueueSize = 256,
            MaxWaitTime = TimeSpan.FromMilliseconds(50),
            QueueFullMode = HyperbatchQueueFullMode.Flush
        },
        TokenCounter = request => request.TokenCount ?? TokenEstimator.Estimate(request.Request.Input),
        ShouldFlush = metrics => metrics.QueueLength >= 64,
        RetryOptions = new HyperbatchRetryOptions
        {
            MaxAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(50),
            MaxDelay = TimeSpan.FromMilliseconds(250),
            JitterFactor = 0.2
        }
    };

    return new HyperbatchScheduler<BatchKey, EmbeddingRequest, EmbeddingResponse>(handler, options);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost(
    "/embeddings",
    async (
        EmbeddingRequest request,
        IHyperbatchScheduler<BatchKey, EmbeddingRequest, EmbeddingResponse> scheduler,
        TimeProvider timeProvider,
        CancellationToken cancellationToken) =>
    {
        if (request.Input.Length == 0 || request.Input.All(string.IsNullOrWhiteSpace))
        {
            return Results.Problem(
                title: "validation_error",
                detail: "Input is required and cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var key = new BatchKey(
            Provider: "OpenAI",
            Operation: "Embeddings",
            Model: request.Model,
            Tenant: request.Tenant ?? "default");

        var tokenCount = request.TokenCount ?? TokenEstimator.Estimate(request.Input);
        var hyperbatchRequest = new HyperbatchRequest<EmbeddingRequest>
        {
            Request = request,
            TokenCount = tokenCount,
            Deadline = timeProvider.GetUtcNow().AddMilliseconds(200)
        };

        var result = await scheduler.EnqueueAsync(key, hyperbatchRequest, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var problem = result.Problem!;
        var statusCode = problem.Code == "hyperbatch.validation_failed"
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status502BadGateway;

        return Results.Problem(title: problem.Code, detail: problem.Description, statusCode: statusCode);
    });

app.Run();
