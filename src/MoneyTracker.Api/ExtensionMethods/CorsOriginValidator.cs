namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Validates whether an origin is permitted during development (localhost and equivalents only).
/// </summary>
public static class CorsOriginValidator
{
    /// <summary>
    /// Returns true for http/https origins pointing at localhost, 127.0.0.1, or any *.dev.localhost subdomain.
    /// </summary>
    public static bool IsAllowedDevelopmentOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme is not ("http" or "https"))
            return false;
        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith(".dev.localhost", StringComparison.OrdinalIgnoreCase);
    }
}
