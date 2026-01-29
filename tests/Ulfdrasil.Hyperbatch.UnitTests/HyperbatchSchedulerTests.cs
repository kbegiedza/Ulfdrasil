using AwesomeAssertions;
using Xunit;

using Ulfdrasil;
using Ulfdrasil.Hyperbatch;
using Ulfdrasil.Hyperbatch.Abstractions;

namespace Ulfdrasil.Hyperbatch.UnitTests;

public class HyperbatchSchedulerTests
{
    [Fact]
    public async Task EnqueueAsync_WhenBatchSizeReached_FlushesSingleBatch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new RecordingHandler();
        var options = new HyperbatchSchedulerOptions<string, int, int>
        {
            DefaultQueueOptions = new HyperbatchQueueOptions
            {
                MaxBatchSize = 3,
                MaxWaitTime = TimeSpan.FromSeconds(5)
            }
        };

        var scheduler = new HyperbatchScheduler<string, int, int>(handler, options);
        try
        {
            var results = await Task.WhenAll(
                scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(1), cancellationToken).AsTask(),
                scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(2), cancellationToken).AsTask(),
                scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(3), cancellationToken).AsTask())
                .ConfigureAwait(true);

            results.Select(result => result.Value).Should().Equal(2, 4, 6);
            handler.Batches.Should().ContainSingle();
            handler.Batches[0].Should().Equal(1, 2, 3);
        }
        finally
        {
            await scheduler.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    public async Task EnqueueAsync_WhenMaxWaitTimeElapsed_FlushesBatch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new RecordingHandler();
        var options = new HyperbatchSchedulerOptions<string, int, int>
        {
            DefaultQueueOptions = new HyperbatchQueueOptions
            {
                MaxWaitTime = TimeSpan.FromMilliseconds(30)
            }
        };

        var scheduler = new HyperbatchScheduler<string, int, int>(handler, options);
        try
        {
            var result = await scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(5), cancellationToken)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(1), cancellationToken)
                .ConfigureAwait(true);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(10);
            handler.Batches.Should().ContainSingle();
        }
        finally
        {
            await scheduler.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    public async Task EnqueueAsync_WhenClientFailureOccurs_BisectsAndReturnsPerItemResults()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new BisectingHandler();
        var options = new HyperbatchSchedulerOptions<string, int, int>
        {
            DefaultQueueOptions = new HyperbatchQueueOptions
            {
                MaxBatchSize = 2,
                MaxWaitTime = TimeSpan.FromSeconds(5)
            }
        };

        var scheduler = new HyperbatchScheduler<string, int, int>(handler, options);
        try
        {
            var results = await Task.WhenAll(
                scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(3), cancellationToken).AsTask(),
                scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(-1), cancellationToken).AsTask())
                .ConfigureAwait(true);

            results[0].IsSuccess.Should().BeTrue();
            results[0].Value.Should().Be(3);
            results[1].IsFailure.Should().BeTrue();
            handler.InvocationCount.Should().BeGreaterThan(1);
        }
        finally
        {
            await scheduler.DisposeAsync().ConfigureAwait(true);
        }
    }

    [Fact]
    public async Task EnqueueAsync_WhenTransientFailureOccurs_RetriesBatch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new RetryHandler();
        var options = new HyperbatchSchedulerOptions<string, int, int>
        {
            DefaultQueueOptions = new HyperbatchQueueOptions
            {
                MaxBatchSize = 1,
                MaxWaitTime = TimeSpan.FromSeconds(5)
            },
            RetryOptions = new HyperbatchRetryOptions
            {
                MaxAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
                MaxDelay = TimeSpan.FromMilliseconds(5),
                JitterFactor = 0
            }
        };

        var scheduler = new HyperbatchScheduler<string, int, int>(handler, options);
        try
        {
            var result = await scheduler.EnqueueAsync("key", HyperbatchRequest<int>.From(8), cancellationToken)
                .AsTask()
                .ConfigureAwait(true);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(8);
            handler.InvocationCount.Should().Be(2);
        }
        finally
        {
            await scheduler.DisposeAsync().ConfigureAwait(true);
        }
    }

    private sealed class RecordingHandler : IHyperbatchBatchHandler<string, int, int>
    {
        public List<IReadOnlyList<int>> Batches { get; } = [];

        public Task<IReadOnlyList<Result<int>>> HandleBatchAsync(
            string key,
            IReadOnlyList<HyperbatchRequest<int>> requests,
            CancellationToken cancellationToken = default)
        {
            Batches.Add(requests.Select(request => request.Request).ToList());
            return Task.FromResult<IReadOnlyList<Result<int>>>(
                requests.Select(request => Result.Success(request.Request * 2)).ToList());
        }
    }

    private sealed class BisectingHandler : IHyperbatchBatchHandler<string, int, int>
    {
        public int InvocationCount { get; private set; }

        public Task<IReadOnlyList<Result<int>>> HandleBatchAsync(
            string key,
            IReadOnlyList<HyperbatchRequest<int>> requests,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;

            if (requests.Count > 1)
            {
                throw new HyperbatchBatchException(
                    HyperbatchBatchFailure.Client(
                        HyperbatchProblems.ValidationFailed("Invalid request in batch."),
                        supportsBisect: true));
            }

            var value = requests[0].Request;
            var result = value < 0
                ? Result.Failure<int>(HyperbatchProblems.ValidationFailed("Value must be non-negative."))
                : Result.Success(value);

            return Task.FromResult<IReadOnlyList<Result<int>>>(new[] { result });
        }
    }

    private sealed class RetryHandler : IHyperbatchBatchHandler<string, int, int>
    {
        public int InvocationCount { get; private set; }

        public Task<IReadOnlyList<Result<int>>> HandleBatchAsync(
            string key,
            IReadOnlyList<HyperbatchRequest<int>> requests,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;

            if (InvocationCount == 1)
            {
                throw new HyperbatchBatchException(
                    HyperbatchBatchFailure.Transient(
                        HyperbatchProblems.BatchFailed("Transient failure.")));
            }

            return Task.FromResult<IReadOnlyList<Result<int>>>(
                requests.Select(request => Result.Success(request.Request)).ToList());
        }
    }
}
