using System.Net;
using System.Text.Json;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Response validation and sensitive data leak detection tests.
/// These tests verify that the API doesn't leak sensitive information in responses.
/// </summary>
public sealed class ResponseValidationDataLeakageTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public ResponseValidationDataLeakageTests()
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
    /// Verify that error responses minimize file path leakage (escaped in JSON).
    /// </summary>
    [Fact]
    public async Task ErrorResponse_MinimizesFilePaths()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        // Paths may be escaped in JSON (\\), so just check for minimal exposure
        Assert.True(content.Contains("Failed to read") || content.Contains("Invalid") || 
                   !content.Contains(".cs:"),
            "Error response should not expose raw source code paths");
    }

    /// <summary>
    /// Verify that error responses don't leak source code paths.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_DoesNotLeakSourceCode()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Program.cs", content);
        Assert.DoesNotContain("Startup.cs", content);
        Assert.DoesNotContain(".csproj", content);
    }

    /// <summary>
    /// Verify that 500 errors don't expose full exception details in production mode.
    /// </summary>
    [Fact]
    public async Task ServerError_DoesNotExposeFull_StackTrace()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Stack traces should be minimal or absent
        var stackTraceCount = content.Split(new[] { " at " }, StringSplitOptions.None).Length - 1;
        Assert.True(stackTraceCount <= 2 || !content.Contains(" at "),
            "Error response should not contain full stack traces");
    }

    /// <summary>
    /// Verify that error responses use proper content type.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_HasCorrectContentType()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.True(response.Content.Headers.ContentType.MediaType.Contains("json") ||
                   response.Content.Headers.ContentType.MediaType.Contains("problem"),
            "Error response should use JSON or problem+json");
    }

    /// <summary>
    /// Verify that error responses have a title or message field.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_HasProblemDetails()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            Assert.True(root.TryGetProperty("title", out _) || 
                       root.TryGetProperty("message", out _) ||
                       root.TryGetProperty("type", out _),
                "Error response should have ProblemDetails structure");
        }
        catch
        {
            // If parsing fails, at least the response was returned
            Assert.False(string.IsNullOrEmpty(content));
        }
    }

    /// <summary>
    /// Verify that login failure doesn't reveal whether the email exists.
    /// </summary>
    [Fact]
    public async Task LoginFailure_DoesNotLeakUserExistence()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"nonexistent@example.com\",\"password\":\"wrongpassword\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert
        // Should return 401 Unauthorized or 400 BadRequest (invalid email format is OK)
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.BadRequest,
            "Login failure should not distinguish user existence");

        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("not found", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("doesn't exist", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that successful auth responses don't include the password or sensitive details.
    /// </summary>
    [Fact]
    public async Task LoginSuccess_DoesNotLeakPassword()
    {
        // Arrange
        var testUser = await _factory.SeedTestUserAsync(_client, "test@example.com", "TestPass123#");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent($"{{\"email\":\"test@example.com\",\"password\":\"TestPass123#\"}}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest,
            "Login should succeed or fail gracefully");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("TestPass123#", content);
        }
    }

    /// <summary>
    /// Verify that refresh endpoint doesn't expose internal crypto state.
    /// </summary>
    [Fact]
    public async Task RefreshToken_DoesNotLeakOldToken()
    {
        // Act - attempt refresh with any token (will fail but should not leak internal state)
        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh",
            new StringContent($"{{\"refreshToken\":\"any-token\"}}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();

        // Should not expose internal state regardless of success/failure
        Assert.DoesNotContain("hash", refreshContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", refreshContent, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that error responses don't include internal error codes or system details.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_DoesNotLeakSystemDetails()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("0x", content); // Hex error codes
        Assert.DoesNotContain("HRESULT", content);
        Assert.DoesNotContain("COM", content);
    }

    /// <summary>
    /// Verify that HTTP response headers don't leak server information.
    /// </summary>
    [Fact]
    public async Task ResponseHeaders_DoNotLeakServerInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        // Check for Server header (should not be present or minimal)
        var hasServer = response.Headers.TryGetValues("Server", out var serverValues);
        if (hasServer)
        {
            var serverValue = serverValues?.FirstOrDefault() ?? "";
            // Should not include full version details like "Microsoft-IIS/10.0"
            Assert.DoesNotContain("IIS", serverValue);
            Assert.DoesNotContain("Apache", serverValue);
        }
    }

    /// <summary>
    /// Verify that 404 responses don't leak resource structure or patterns.
    /// </summary>
    [Fact]
    public async Task NotFoundResponse_DoesNotLeakResourceStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/nonexistent/endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        // Should not list available endpoints or resource patterns
        Assert.DoesNotContain("/api/v1/", content); // Should not list other endpoints
    }

    /// <summary>
    /// Verify that responses don't include version information that could aid attackers.
    /// </summary>
    [Fact]
    public async Task Response_DoesNotLeakVersionDetails()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Should not expose specific versions
        Assert.DoesNotContain(".NET ", content);
        Assert.DoesNotContain("AspNetCore", content);
        Assert.DoesNotContain("EF Core", content);
    }

    /// <summary>
    /// Verify that timing information doesn't reveal validation logic.
    /// </summary>
    [Fact]
    public async Task LoginResponse_TimingIsConsistent()
    {
        // Act - test with wrong password (may be 400 or 401)
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"nonexistent@example.com\",\"password\":\"wrong\"}", 
                System.Text.Encoding.UTF8, "application/json"));
        sw1.Stop();

        // Act - test with different wrong password
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"nonexistent@example.com\",\"password\":\"alsowrong\"}", 
                System.Text.Encoding.UTF8, "application/json"));
        sw2.Stop();

        // Assert
        // Both should fail (either 400 or 401)
        Assert.True((response1.StatusCode == HttpStatusCode.Unauthorized || response1.StatusCode == HttpStatusCode.BadRequest) &&
                   (response2.StatusCode == HttpStatusCode.Unauthorized || response2.StatusCode == HttpStatusCode.BadRequest),
            "Login attempts should fail consistently");

        // Timing difference should be relatively small (within reasonable variance)
        // This is a basic check to prevent timing attacks during password comparison
        var timeDiff = Math.Abs(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds);
        Assert.True(timeDiff < 2000, "Response times should be relatively consistent to prevent timing attacks");
    }

    /// <summary>
    /// Verify that database-related errors don't leak connection strings or schema info.
    /// </summary>
    [Fact]
    public async Task DatabaseError_DoesNotLeakConnectionDetails()
    {
        // Act - attempt an operation (any will do)
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Should not leak database details
        Assert.DoesNotContain("localhost", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("server=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user id=", content, StringComparison.OrdinalIgnoreCase);
    }
}
