namespace MoneyTracker.Api.ExtensionMethods;

/// <summary>
/// Applies no-store cache headers to HTTP responses containing auth tokens.
/// Extracted to allow direct unit testing of the header values.
/// </summary>
public static class NoStoreResponseHeaders
{
    /// <summary>
    /// Writes Cache-Control, Pragma, and Expires headers that prevent caching of auth token responses.
    /// </summary>
    public static void Apply(HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }
}
