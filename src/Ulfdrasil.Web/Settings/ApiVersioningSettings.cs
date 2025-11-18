using Asp.Versioning;

namespace Ulfdrasil.Web.Settings;

/// <summary>
/// Settings for API versioning.
/// </summary>
public sealed class ApiVersioningSettings
{
    /// <summary>
    /// List of supported API versions.
    /// </summary>
    public required List<ApiVersion> ApiVersions { get; init; }
}