using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Exsaga.Console.EmbeddingsLoad;

/// <summary>
/// Runs parallel workers that post embedding requests to the hyperbatching example.
/// </summary>
public sealed class EmbeddingsLoadDaemon : BackgroundService
{
    private readonly ILogger<EmbeddingsLoadDaemon> _logger;
    private readonly EmbeddingsLoadSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<int, long> _statusCodes = new();
    private long _successCount;
    private long _failureCount;
    private long _requestCount;

    public EmbeddingsLoadDaemon(
        ILogger<EmbeddingsLoadDaemon> logger,
        EmbeddingsLoadSettings settings,
        TimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = ValidateSettings(_settings);
        var inputs = ResolveInputs(settings);

        var baseUri = CreateBaseUri(settings.BaseUrl);
        var endpointUri = new Uri(baseUri, settings.Endpoint);

        using var httpClient = new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds))
        };

        using var durationCts = CreateDurationTokenSource(settings);
        using var linkedCts = CreateLinkedTokenSource(stoppingToken, durationCts);
        var token = linkedCts?.Token ?? stoppingToken;

        var startedAt = _timeProvider.GetUtcNow();

        _logger.LogInformation(
            "Starting embeddings load: workers={Workers}, requestsPerWorker={RequestsPerWorker}, durationSeconds={DurationSeconds}, delayMs={DelayMs}, baseUrl={BaseUrl}, endpoint={Endpoint}",
            settings.WorkerCount,
            settings.RequestsPerWorker,
            settings.DurationSeconds,
            settings.DelayMs,
            baseUri,
            endpointUri);

        var tasks = new Task[settings.WorkerCount];
        for (var i = 0; i < settings.WorkerCount; i++)
        {
            var workerId = i;
            tasks[i] = RunWorkerAsync(workerId, settings, inputs, httpClient, endpointUri, token);
        }

        await Task.WhenAll(tasks);

        var finishedAt = _timeProvider.GetUtcNow();
        var elapsed = finishedAt - startedAt;

        _logger.LogInformation(
            "Embeddings load finished: requests={Requests}, success={Success}, failure={Failure}, elapsedMs={ElapsedMs}",
            Interlocked.Read(ref _requestCount),
            Interlocked.Read(ref _successCount),
            Interlocked.Read(ref _failureCount),
            elapsed.TotalMilliseconds);

        if (!_statusCodes.IsEmpty)
        {
            _logger.LogInformation("Response status breakdown: {StatusBreakdown}", FormatStatusBreakdown());
        }
    }

    private async Task RunWorkerAsync(
        int workerId,
        EmbeddingsLoadSettings settings,
        string[] inputs,
        HttpClient httpClient,
        Uri endpointUri,
        CancellationToken cancellationToken)
    {
        var sent = 0;
        var success = 0;
        var failure = 0;
        var requestLimit = settings.RequestsPerWorker;
        var delay = settings.DelayMs > 0 ? TimeSpan.FromMilliseconds(settings.DelayMs) : TimeSpan.Zero;

        _logger.LogInformation(
            "Worker {WorkerId} started with requestLimit={RequestLimit}",
            workerId,
            requestLimit);

        while (!cancellationToken.IsCancellationRequested && (requestLimit <= 0 || sent < requestLimit))
        {
            sent++;
            Interlocked.Increment(ref _requestCount);

            var request = CreateRequest(settings, inputs);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var response = await httpClient.PostAsJsonAsync(endpointUri, request, cancellationToken);
                stopwatch.Stop();

                TrackStatusCode(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    success++;
                    Interlocked.Increment(ref _successCount);

                    _logger.LogDebug(
                        "Worker {WorkerId} request {RequestIndex} succeeded in {ElapsedMs}ms",
                        workerId,
                        sent,
                        stopwatch.Elapsed.TotalMilliseconds);
                }
                else
                {
                    failure++;
                    Interlocked.Increment(ref _failureCount);

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Worker {WorkerId} request {RequestIndex} failed with {StatusCode}: {Body}",
                        workerId,
                        sent,
                        (int)response.StatusCode,
                        body);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                failure++;
                Interlocked.Increment(ref _failureCount);
                _logger.LogWarning(
                    ex,
                    "Worker {WorkerId} request {RequestIndex} threw after {ElapsedMs}ms",
                    workerId,
                    sent,
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        _logger.LogInformation(
            "Worker {WorkerId} finished: sent={Sent}, success={Success}, failure={Failure}",
            workerId,
            sent,
            success,
            failure);
    }

    private static EmbeddingsLoadSettings ValidateSettings(EmbeddingsLoadSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.BaseUrl must be provided.");
        }

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.Endpoint must be provided.");
        }

        if (settings.WorkerCount <= 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.WorkerCount must be greater than zero.");
        }

        if (settings.RequestsPerWorker < 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.RequestsPerWorker must be zero or greater.");
        }

        if (settings.DurationSeconds is < 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.DurationSeconds must be zero or greater.");
        }

        if (settings.DelayMs < 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.DelayMs must be zero or greater.");
        }

        if (settings.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.TimeoutSeconds must be greater than zero.");
        }

        return settings;
    }

    private static string[] ResolveInputs(EmbeddingsLoadSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Input))
        {
            return [settings.Input];
        }

        if (settings.Inputs is null || settings.Inputs.Length == 0)
        {
            throw new InvalidOperationException("EmbeddingsLoadSettings.Inputs must contain at least one entry.");
        }

        return settings.Inputs;
    }

    private static Uri CreateBaseUri(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"EmbeddingsLoadSettings.BaseUrl is not a valid absolute URI: {baseUrl}");
        }

        EnsureSupportedScheme(uri, "EmbeddingsLoadSettings.BaseUrl", baseUrl);

        return uri;
    }

    private static Uri CreateEndpointUri(string endpoint, Uri baseUri)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
        {
            EnsureSupportedScheme(absolute, "EmbeddingsLoadSettings.Endpoint", endpoint);
            return absolute;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Relative, out var relative))
        {
            throw new InvalidOperationException($"EmbeddingsLoadSettings.Endpoint is not a valid URI: {endpoint}");
        }

        return new Uri(baseUri, relative);
    }

    private static void EnsureSupportedScheme(Uri uri, string settingName, string rawValue)
    {
        if (uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"{settingName} must use http or https, but was '{rawValue}'.");
        }
    }

    private static CancellationTokenSource? CreateDurationTokenSource(EmbeddingsLoadSettings settings)
    {
        return settings.DurationSeconds is > 0
            ? new CancellationTokenSource(TimeSpan.FromSeconds(settings.DurationSeconds.Value))
            : null;
    }

    private static CancellationTokenSource? CreateLinkedTokenSource(
        CancellationToken stoppingToken,
        CancellationTokenSource? durationCts)
    {
        return durationCts is null
            ? null
            : CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, durationCts.Token);
    }

    private static EmbeddingRequest CreateRequest(EmbeddingsLoadSettings settings, string[] inputs)
    {
        var input = inputs[Random.Shared.Next(inputs.Length)];

        return new EmbeddingRequest
        {
            Input = [input],
            Model = settings.Model,
            Tenant = settings.Tenant,
            Dimensions = settings.Dimensions
        };
    }

    private void TrackStatusCode(System.Net.HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        _statusCodes.AddOrUpdate(code, 1, (_, count) => count + 1);
    }

    private string FormatStatusBreakdown()
    {
        return string.Join(
            ", ",
            _statusCodes
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={pair.Value}"));
    }
}
