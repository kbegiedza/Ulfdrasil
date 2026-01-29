using System.Collections.Generic;

namespace Exsaga.Console.EmbeddingsLoad;

/// <summary>
/// Configuration settings for the embeddings load generator.
/// </summary>
public sealed record EmbeddingsLoadSettings
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = nameof(EmbeddingsLoadSettings);

    /// <summary>
    /// Gets the base URL for the embeddings endpoint.
    /// </summary>
    public string BaseUrl { get; init; } = "http://localhost:5000";

    /// <summary>
    /// Gets the relative or absolute embeddings endpoint path.
    /// </summary>
    public string Endpoint { get; init; } = "/embeddings";

    /// <summary>
    /// Gets the number of parallel workers to run.
    /// </summary>
    public int WorkerCount { get; init; } = 4;

    /// <summary>
    /// Gets the number of requests each worker should send before stopping. Use 0 to run until cancelled.
    /// </summary>
    public int RequestsPerWorker { get; init; } = 100;

    /// <summary>
    /// Gets the optional duration (seconds) to run before stopping.
    /// </summary>
    public int? DurationSeconds { get; init; }

    /// <summary>
    /// Gets the delay between requests in milliseconds.
    /// </summary>
    public int DelayMs { get; init; }

    /// <summary>
    /// Gets the HTTP timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the model name to use for embedding requests.
    /// </summary>
    public string Model { get; init; } = "text-embedding-3-small";

    /// <summary>
    /// Gets the tenant identifier to send with requests.
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Gets the optional output vector dimensions.
    /// </summary>
    public int? Dimensions { get; init; }

    /// <summary>
    /// Gets an optional single input override provided via CLI.
    /// </summary>
    public string? Input { get; init; }

    /// <summary>
    /// Gets the list of possible inputs to sample from.
    /// </summary>
    public string[] Inputs { get; init; } =
    [
        "hello world",
        "the quick brown fox jumps over the lazy dog",
        "exsaga hyperbatching load test"
    ];

    /// <summary>
    /// Gets the command line mappings for shorthand CLI parameters.
    /// </summary>
    public static IDictionary<string, string> CommandLineMappings { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["--base-url"] = $"{SectionName}:BaseUrl",
            ["--endpoint"] = $"{SectionName}:Endpoint",
            ["--workers"] = $"{SectionName}:WorkerCount",
            ["--requests-per-worker"] = $"{SectionName}:RequestsPerWorker",
            ["--duration-seconds"] = $"{SectionName}:DurationSeconds",
            ["--delay-ms"] = $"{SectionName}:DelayMs",
            ["--timeout-seconds"] = $"{SectionName}:TimeoutSeconds",
            ["--model"] = $"{SectionName}:Model",
            ["--tenant"] = $"{SectionName}:Tenant",
            ["--dimensions"] = $"{SectionName}:Dimensions",
            ["--input"] = $"{SectionName}:Input"
        };
}
