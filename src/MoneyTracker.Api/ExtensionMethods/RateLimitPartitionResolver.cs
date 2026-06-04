namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Resolves the partition key used to bucket rate-limit counters per HTTP request.
/// Extracted to allow direct unit testing without a full HTTP pipeline.
/// </summary>
public static class RateLimitPartitionResolver
{
    /// <summary>
    /// Returns the remote IP address string, or <c>"unknown"</c> when the address is unavailable.
    /// </summary>
    public static string Resolve(Microsoft.AspNetCore.Http.HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
