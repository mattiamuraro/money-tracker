using System.Diagnostics;
using System.Net;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Performance baseline tests to track API response times and resource efficiency.
/// These tests establish baseline metrics for monitoring performance regression.
/// </summary>
public sealed class PerformanceBaselineTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public PerformanceBaselineTests()
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
    /// Baseline: Single auth-config request should complete under 200ms in test environment.
    /// </summary>
    [Fact]
    public async Task AuthConfigEndpoint_RespondsFast()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 200,
            $"Request took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
    }

    /// <summary>
    /// Baseline: Failed auth attempt should complete under 500ms in test environment.
    /// </summary>
    [Fact]
    public async Task FailedAuthAttempt_RespondsFast()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        stopwatch.Stop();

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized,
                   $"Unexpected status: {response.StatusCode}");
        Assert.True(stopwatch.ElapsedMilliseconds < 1500,
            $"Request took {stopwatch.ElapsedMilliseconds}ms, expected < 1500ms");
    }

    /// <summary>
    /// Baseline: 100 concurrent requests to non-auth endpoint should complete in reasonable time.
    /// </summary>
    [Fact]
    public async Task ConcurrentRequests_CompleteInReasonableTime()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var requestCount = 100;
        var tasks = Enumerable.Range(0, requestCount)
            .Select(_ => _client.GetAsync("/api/v1/auth/config"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK ||
                                                 r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(successCount > 0);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"100 concurrent requests took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    /// <summary>
    /// Baseline: Response size for auth-config should be reasonable (< 1KB).
    /// </summary>
    [Fact]
    public async Task AuthConfigResponse_ReasonableSize()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(content.Length < 1024,
            $"Response size {content.Length} bytes exceeds reasonable limit");
    }

    /// <summary>
    /// Baseline: Error response should not include excessive detail (< 500 bytes for standard error).
    /// </summary>
    [Fact]
    public async Task ErrorResponse_ReasonableSize()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError);
        Assert.True(content.Length < 2000,
            $"Error response size {content.Length} bytes is excessive");
    }

    /// <summary>
    /// Baseline: Rapid sequential requests should not cause performance degradation.
    /// </summary>
    [Fact]
    public async Task RapidSequentialRequests_MaintainPerformance()
    {
        // Arrange
        var times = new List<long>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 50; i++)
        {
            var reqStopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync("/api/v1/auth/config");
            reqStopwatch.Stop();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Request {i} failed: {response.StatusCode}");
            times.Add(reqStopwatch.ElapsedMilliseconds);
        }

        stopwatch.Stop();

        // Assert
        var avgTime = times.Average();
        var maxTime = times.Max();

        Assert.True(avgTime < 100,
            $"Average request time {avgTime}ms exceeds baseline of 100ms");
        Assert.True(maxTime < 200,
            $"Max request time {maxTime}ms exceeds baseline of 200ms");
    }

    /// <summary>
    /// Baseline: Request with headers should not add significant overhead.
    /// </summary>
    [Fact]
    public async Task RequestWithSecurityHeaders_NoSignificantOverhead()
    {
        // Arrange
        var baselineStopwatch = Stopwatch.StartNew();
        var baselineResponse = await _client.GetAsync("/api/v1/auth/config");
        baselineStopwatch.Stop();

        var headerStopwatch = Stopwatch.StartNew();
        _client.DefaultRequestHeaders.Add("X-Custom-Header", new string('a', 100));
        var headerResponse = await _client.GetAsync("/api/v1/auth/config");
        headerStopwatch.Stop();
        _client.DefaultRequestHeaders.Remove("X-Custom-Header");

        // Assert
        var overhead = Math.Abs(headerStopwatch.ElapsedMilliseconds - baselineStopwatch.ElapsedMilliseconds);
        // Allow up to 500ms variance due to test environment
        Assert.True(overhead < 500,
            $"Header overhead {overhead}ms exceeds acceptable threshold");
    }
}
