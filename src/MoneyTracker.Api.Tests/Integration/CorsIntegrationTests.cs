using System.Net;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Verifies that CORS origin policies are enforced correctly by the running API host.
/// In Development mode the API applies <see cref="MoneyTracker.Api.ExtensionMethods.CorsOriginValidator"/>
/// rules: only localhost, 127.0.0.1, and *.dev.localhost origins are permitted.
/// </summary>
public sealed class CorsIntegrationTests : IDisposable
{
    private readonly ApiWebFactory _factory;

    public CorsIntegrationTests()
    {
        _factory = new ApiWebFactory();
    }

    public void Dispose() => _factory.Dispose();

    // ── allowed development origins ──────────────────────────────────────────

    /// <summary>
    /// A pre-flight from http://localhost is allowed in Development mode.
    /// </summary>
    [Fact]
    public async Task Preflight_FromLocalhost_IsAllowed()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected Access-Control-Allow-Origin header for localhost");
    }

    /// <summary>
    /// A pre-flight from http://localhost with an explicit port is allowed.
    /// </summary>
    [Fact]
    public async Task Preflight_FromLocalhostWithPort_IsAllowed()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected Access-Control-Allow-Origin for localhost:3000");
    }

    /// <summary>
    /// A pre-flight from http://127.0.0.1 is allowed in Development mode.
    /// </summary>
    [Fact]
    public async Task Preflight_From127001_IsAllowed()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://127.0.0.1");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected Access-Control-Allow-Origin for 127.0.0.1");
    }

    /// <summary>
    /// A pre-flight from a *.dev.localhost subdomain is allowed in Development mode.
    /// </summary>
    [Fact]
    public async Task Preflight_FromDevLocalhostSubdomain_IsAllowed()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://app.dev.localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected Access-Control-Allow-Origin for *.dev.localhost");
    }

    // ── disallowed origins ───────────────────────────────────────────────────

    /// <summary>
    /// A pre-flight from an arbitrary external origin is rejected in Development mode.
    /// </summary>
    [Fact]
    public async Task Preflight_FromExternalOrigin_IsRejected()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "https://evil.example.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected no Access-Control-Allow-Origin for external origin in Development mode");
    }

    /// <summary>
    /// A pre-flight from a domain that merely contains 'localhost' as a substring but is not
    /// localhost itself is rejected (e.g., evilocalhost.com).
    /// </summary>
    [Fact]
    public async Task Preflight_FromLocalhostSpoofDomain_IsRejected()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "https://evilocalhost.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected no Access-Control-Allow-Origin for spoofed localhost domain");
    }

    /// <summary>
    /// A pre-flight from a *.localhost subdomain that is not *.dev.localhost is rejected.
    /// </summary>
    [Fact]
    public async Task Preflight_FromNonDevLocalhostSubdomain_IsRejected()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://app.localhost");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected no Access-Control-Allow-Origin for non-dev.localhost subdomain");
    }

    // ── cross-origin simple request ──────────────────────────────────────────

    /// <summary>
    /// A simple (non-pre-flight) GET request from localhost receives the CORS header on the response.
    /// </summary>
    [Fact]
    public async Task SimpleRequest_FromLocalhost_ReceivesCorsHeader()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/forecast-recurrence-rule-types");
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected CORS header on response to simple request from localhost");
    }

    /// <summary>
    /// A simple GET request from an external origin does not receive a CORS allow-origin header.
    /// </summary>
    [Fact]
    public async Task SimpleRequest_FromExternalOrigin_DoesNotReceiveCorsHeader()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/forecast-recurrence-rule-types");
        request.Headers.Add("Origin", "https://attacker.io");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected no CORS header for external origin");
    }
}
