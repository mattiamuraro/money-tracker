namespace MoneyTracker.Api.Options;

/// <summary>
/// Configures trusted reverse proxy forwarding behavior.
/// </summary>
public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    /// <summary>
    /// Trusted reverse proxy IP addresses.
    /// </summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>
    /// Trusted reverse proxy networks in CIDR notation.
    /// </summary>
    public string[] KnownNetworks { get; set; } = [];

    /// <summary>
    /// Maximum number of entries in X-Forwarded-* headers to process.
    /// </summary>
    public int ForwardLimit { get; set; } = 2;
}
