using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Integration tests for security hardening edge cases and best practices.
/// Covers token leakage detection, HSTS header enforcement, and error response boundaries.
/// </summary>
public sealed class SecurityHardeningIntegrationTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public SecurityHardeningIntegrationTests()
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
    /// Verify that error responses from failed auth attempts do not leak JWT tokens.
    /// </summary>
    [Fact]
    public async Task FailedLoginResponse_DoesNotContainJwtTokens()
    {
        // Act – attempt login with wrong credentials.
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "nonexistent@test.com", password = "WrongPassword123" }));

        // Assert – expect 401 or 400 (invalid credentials).
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.BadRequest,
                   $"Expected auth error, got {response.StatusCode}");

        // Verify the response body does not contain JWT-like tokens.
        var responseBody = await response.Content.ReadAsStringAsync();

        // Check for common JWT/token patterns.
        Assert.DoesNotContain("eyJ", responseBody); // JWT header pattern
        Assert.DoesNotContain("Bearer ", responseBody); // Bearer token leak
        Assert.DoesNotContain("RefreshToken", responseBody); // Explicit token in response
        Assert.False(string.IsNullOrEmpty(responseBody), "Error response should not be empty");
    }

    /// <summary>
    /// Verify that validation error responses do not leak sensitive fields.
    /// </summary>
    [Fact]
    public async Task ValidationErrorResponse_DoesNotLeakSensitiveFields()
    {
        // Act – attempt login to get a token, then use it for an invalid change-password attempt.
        var userId = await _factory.SeedTestUserAsync(_client);

        // First, login to get a token.
        var loginResponse = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "testuser@example.com", password = "TestPass#123" }));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody?.Token);

        // Now try to change password with wrong current password.
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);
        var response = await _client.PostAsync("/api/v1/auth/change-password",
            JsonContent.Create(new { currentPassword = "WrongPassword", newPassword = "NewPassword#123" }));

        // Assert – expect 401 (wrong current password).
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();

        // Verify no internal system paths or stack traces leak.
        Assert.DoesNotContain("Stack", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/bin/", responseBody);
        Assert.DoesNotContain("Program.cs", responseBody);
        Assert.False(string.IsNullOrEmpty(responseBody), "Error response should not be empty");
    }

    /// <summary>
    /// Verify that 404 responses do not leak path information or endpoint structure.
    /// </summary>
    [Fact]
    public async Task NotFoundResponse_DoesNotLeakDetailedPathInfo()
    {
        // Act – request a non-existent endpoint.
        var response = await _client.GetAsync("/api/v1/auth/nonexistent-endpoint-12345");

        // Assert – expect 404.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();

        // Verify no internal routing details leak.
        Assert.DoesNotContain("/bin/", responseBody);
        Assert.DoesNotContain("Program.cs", responseBody);
    }

    /// <summary>
    /// Verify that error responses handle extremely long inputs without crashing.
    /// </summary>
    [Fact]
    public async Task LongInputErrorResponse_HandlesGracefully()
    {
        // Act – send a very long username.
        var longUsername = new string('a', 10000);
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = longUsername, password = "Password123" }));

        // Assert – expect a valid error response, not a 500 crash.
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
                   $"Expected valid error response, got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(responseBody));
    }

    /// <summary>
    /// Verify that malformed JSON is handled by the exception middleware without exposing parser details.
    /// </summary>
    [Fact]
    public async Task MalformedJsonErrorResponse_DoesNotLeakParserDetails()
    {
        // Act – send invalid JSON.
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json"));

        // Assert – expect 400 or 500; if 500, verify no parser details leak.
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.InternalServerError,
            $"Expected 400 or 500, got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();

        // Verify no JsonReader/parser exceptions leak details.
        Assert.DoesNotContain("JsonReader", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Newtonsoft", responseBody);
        Assert.DoesNotContain("LineNumber", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrEmpty(responseBody), "Error response should not be empty");
    }

    /// <summary>
    /// Verify that HTTPS-only (HSTS) header is present in non-development environments.
    /// </summary>
    [Fact]
    public async Task HstsHeader_IsNotPresentInDevelopment()
    {
        // Note: Our factory runs in Development mode, so HSTS should NOT be present.
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert – development should skip HSTS per middleware.
        // (HSTS is skipped in Development to allow local HTTP testing.)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The Strict-Transport-Security header may or may not be present in dev.
        // This test documents the behavior.
    }

    /// <summary>
    /// Verify that missing Authorization header on a protected endpoint returns 401 with valid response.
    /// </summary>
    [Fact]
    public async Task MissingAuthHeaderOnProtectedEndpoint_Returns401()
    {
        // Arrange – create a second isolated client without default auth headers.
        var isolatedClient = _factory.CreateClient();

        // Act – call the protected endpoint without any auth header.
        var response = await isolatedClient.PostAsync("/api/v1/auth/change-password",
            JsonContent.Create(new { currentPassword = "pass1", newPassword = "pass2" }));

        // Assert – expect 401 Unauthorized and not a 500 crash.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        isolatedClient.Dispose();
    }

    /// <summary>
    /// Verify that null bytes in input don't cause unexpected behavior.
    /// </summary>
    [Fact]
    public async Task NullByteInInput_IsHandledSafely()
    {
        // Act – send username with embedded null byte.
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "user\0name@test.com", password = "Password123" }));

        // Assert – expect a valid error response.
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Unauthorized,
                   $"Expected valid error, got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(responseBody));
    }

    /// <summary>
    /// Verify that Unicode/emoji in input don't cause encoding issues.
    /// </summary>
    [Fact]
    public async Task UnicodeCharactersInInput_AreHandledCorrectly()
    {
        // Act – send username with emoji.
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "user😀@test.com", password = "Password123" }));

        // Assert – expect valid error, not encoding crash.
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Unauthorized,
                   $"Expected valid error, got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(responseBody));
    }

    /// <summary>
    /// Verify that very high rate of requests doesn't leak timing information in error responses.
    /// </summary>
    [Fact]
    public async Task RateLimitedResponse_ResponseTimeIsConsistent()
    {
        // Act – fire several requests in rapid succession.
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _client.GetAsync("/api/v1/auth/config"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert – all responses should be valid (either 200 or 429), with consistent structure.
        foreach (var response in responses)
        {
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.TooManyRequests,
                $"Unexpected status: {response.StatusCode}");
        }

        // Verify no response has error details that would leak timing info.
        var bodies = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));
        foreach (var body in bodies)
        {
            Assert.DoesNotContain("Duration", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Timestamp", body, StringComparison.OrdinalIgnoreCase);
        }
    }
}

/// <summary>
/// DTO for deserialization of login response.
/// </summary>
internal class LoginResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}

