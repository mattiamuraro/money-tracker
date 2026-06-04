using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

// Each test creates its own factory so rate-limit buckets are always fresh.
public sealed class RateLimitingIntegrationTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests()
    {
        _factory = new ApiWebFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task LoginEndpoint_WhenCalledBeyondRateLimit_Returns429()
    {
        // The auth-login policy allows 10 requests per minute per IP.
        // TestServer uses 127.0.0.1, so all requests share one bucket.
        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 11; i++)
        {
            var content = JsonContent.Create(new { username = "probe@test.com", password = "probe" });
            lastResponse = await _client.PostAsync("/api/v1/auth/login", content);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_WhenCalledWithinRateLimit_DoesNotReturn429()
    {
        // 5 requests — well below the 10/min limit.
        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 5; i++)
        {
            var content = JsonContent.Create(new { username = "probe@test.com", password = "probe" });
            lastResponse = await _client.PostAsync("/api/v1/auth/login", content);
        }

        // Expect 400/401 from the handler, not 429 from the limiter.
        Assert.NotEqual(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
    }

    [Fact]
    public async Task RejectedRateLimitResponse_ReturnsProblemJsonContentType()
    {
        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 11; i++)
        {
            var content = JsonContent.Create(new { username = "probe@test.com", password = "probe" });
            lastResponse = await _client.PostAsync("/api/v1/auth/login", content);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
        Assert.Equal("application/problem+json", lastResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RejectedRateLimitResponse_BodyContainsTooManyRequestsTitle()
    {
        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 11; i++)
        {
            var content = JsonContent.Create(new { username = "probe@test.com", password = "probe" });
            lastResponse = await _client.PostAsync("/api/v1/auth/login", content);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);

        var body = await lastResponse.Content.ReadFromJsonAsync<RateLimitProblemResponse>();
        Assert.NotNull(body);
        Assert.Equal("Too Many Requests", body.Title);
        Assert.Equal(429, body.Status);
    }

    [Fact]
    public async Task GlobalLimiter_WhenExceeded_Returns429()
    {
        // The global policy allows 120 requests per minute per IP.
        // We use GET /config which has no specific rate-limit policy, so it uses the global limiter.
        // Fire 121 requests to exceed the budget.
        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 121; i++)
        {
            lastResponse = await _client.GetAsync("/api/v1/auth/config");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
    }

    [Fact]
    public async Task GlobalLimiter_WhenNotExceeded_AllowsRequests()
    {
        // Fire 100 requests to GET /config, well below the 120/min global limit.
        int non429Count = 0;
        for (var i = 0; i < 100; i++)
        {
            var response = await _client.GetAsync("/api/v1/auth/config");

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                non429Count++;
        }

        // Assert that we didn't hit the rate limiter.
        Assert.Equal(100, non429Count);
    }

    private sealed record RateLimitProblemResponse(string Title, int Status, string Detail);
}
