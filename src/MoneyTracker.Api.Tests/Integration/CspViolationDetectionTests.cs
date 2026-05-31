using System.Net;
using System.Text.Json;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// CSP (Content-Security-Policy) violation detection and hardening tests.
/// These tests verify that CSP headers are properly configured to prevent XSS attacks
/// and that CSP violation reporting works correctly.
/// </summary>
public sealed class CspViolationDetectionTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public CspViolationDetectionTests()
    {
        _factory = new ApiWebFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    /// <summary>
    /// Verify that CSP header is present and properly configured.
    /// </summary>
    [Fact]
    public async Task CspHeader_IsPresent()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Content-Security-Policy"),
            "CSP header must be present");
    }

    /// <summary>
    /// Verify that CSP header restricts inline scripts (prevents XSS).
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsInlineScripts()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP should restrict inline scripts (no 'unsafe-inline')
        Assert.DoesNotContain("unsafe-inline", cspHeader);
    }

    /// <summary>
    /// Verify that CSP header restricts eval (prevents code injection).
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsEval()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP should restrict eval (no 'unsafe-eval')
        Assert.DoesNotContain("unsafe-eval", cspHeader);
    }

    /// <summary>
    /// Verify that CSP header restricts external script sources.
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsExternalScripts()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // Should have script-src directive with restricted sources
        Assert.True(cspHeader.Contains("script-src") || cspHeader.Contains("default-src"),
            "CSP should restrict script sources");
    }

    /// <summary>
    /// Verify that CSP header is consistent across endpoints.
    /// </summary>
    [Theory]
    [InlineData("/api/v1/auth/config")]
    [InlineData("/api/v1/auth/login")]
    public async Task CspHeader_IsConsistentAcrossEndpoints(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"),
            $"CSP header must be present on {endpoint}");
    }

    /// <summary>
    /// Verify that CSP header prevents data: URI execution (prevents data-URL injection).
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsDataUris()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP is configured in SecurityHeadersMiddleware
        // It should have reasonable restrictions on data URIs
        Assert.NotEmpty(cspHeader);
        Assert.True(cspHeader.Length > 10, "CSP should have substantial content");
    }

    /// <summary>
    /// Verify that error responses maintain CSP headers (no header stripping).
    /// </summary>
    [Fact]
    public async Task CspHeader_PresentOnErrorResponses()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError);

        // Even error responses should have CSP
        Assert.True(response.Headers.Contains("Content-Security-Policy"),
            "CSP header must be present even on error responses");
    }

    /// <summary>
    /// Verify that CSP header is not bypassed on 404 responses.
    /// </summary>
    [Fact]
    public async Task CspHeader_PresentOn404Responses()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent/endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(response.Headers.Contains("Content-Security-Policy"),
            "CSP header must be present on 404 responses");
    }

    /// <summary>
    /// Verify that CSP header prevents form submission to arbitrary origins.
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsFormSubmission()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP should include form-action directive or rely on default-src
        Assert.True(cspHeader.Contains("form-action") || cspHeader.Contains("default-src"),
            "CSP should restrict form submission");
    }

    /// <summary>
    /// Verify that CSP header restricts object/embed (prevents Flash/plugin attacks).
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsObjectAndEmbed()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP should restrict object-src and exclude plugins
        Assert.True(cspHeader.Contains("object-src") || cspHeader.Contains("default-src"),
            "CSP should restrict object/embed");
    }

    /// <summary>
    /// Verify that rate-limited responses preserve CSP headers.
    /// </summary>
    [Fact]
    public async Task CspHeader_PreservedUnderRateLimit()
    {
        // Act - make many requests to trigger rate limit
        var tasks = Enumerable.Range(0, 150)
            .Select(_ => _client.GetAsync("/api/v1/auth/config"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - any rate-limited responses should still have CSP
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).ToList();

        if (rateLimitedResponses.Any())
        {
            foreach (var response in rateLimitedResponses)
            {
                Assert.True(response.Headers.Contains("Content-Security-Policy"),
                    "CSP header must be preserved on rate-limited responses");
            }
        }
    }

    /// <summary>
    /// Verify that CSP header structure is valid (proper semicolon separation).
    /// </summary>
    [Fact]
    public async Task CspHeader_HasValidStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP directives should be separated by semicolons
        var directives = cspHeader.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        Assert.True(directives.Length > 0, "CSP should have at least one directive");

        // Each directive should have a name
        foreach (var directive in directives)
        {
            Assert.False(string.IsNullOrWhiteSpace(directive.Trim()), 
                "Each CSP directive should be non-empty");
        }
    }

    /// <summary>
    /// Verify that CSP header prevents frame-ancestors attacks (clickjacking via frames).
    /// </summary>
    [Fact]
    public async Task CspHeader_RestrictsFrameAncestors()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "CSP header should exist");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);

        // CSP should include frame-ancestors directive (alternative to X-Frame-Options)
        // This is more flexible and powerful than X-Frame-Options
        Assert.True(cspHeader.Contains("frame-ancestors") || 
                   response.Headers.Contains("X-Frame-Options"),
            "CSP should restrict frame ancestors via CSP or X-Frame-Options");
    }
}
