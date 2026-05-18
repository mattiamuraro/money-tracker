namespace MoneyTracker.Api.Options;

/// <summary>
/// Configures Kestrel request limits for API hardening.
/// </summary>
public sealed class RequestLimitsOptions
{
    public const string SectionName = "Security:RequestLimits";

    /// <summary>
    /// Maximum allowed request body size in bytes.
    /// </summary>
    public long MaxRequestBodySizeBytes { get; set; } = 1_048_576;

    /// <summary>
    /// Maximum total size of all request headers in bytes.
    /// </summary>
    public int MaxRequestHeadersTotalSizeBytes { get; set; } = 32_768;
}
