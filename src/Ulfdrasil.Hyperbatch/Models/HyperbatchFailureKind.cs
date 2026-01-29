namespace Ulfdrasil.Hyperbatch;

/// <summary>
/// Categorizes batch-level failures for retry and bisect behavior.
/// </summary>
public enum HyperbatchFailureKind
{
    /// <summary>
    /// Client-side validation or request errors.
    /// </summary>
    Client,

    /// <summary>
    /// Transient upstream failures that can be retried.
    /// </summary>
    Transient,

    /// <summary>
    /// Transport or connectivity failures.
    /// </summary>
    Transport,

    /// <summary>
    /// Unknown failures.
    /// </summary>
    Unknown
}
