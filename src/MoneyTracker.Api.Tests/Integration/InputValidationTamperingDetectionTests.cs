using System.Net;
using System.Text;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Input validation and parameter tampering detection tests.
/// These tests verify that the API properly validates inputs and detects/rejects tampered requests.
/// </summary>
public sealed class InputValidationTamperingDetectionTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public InputValidationTamperingDetectionTests()
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
    /// Verify that malformed JSON is rejected with 400 or 500.
    /// </summary>
    [Fact]
    public async Task MalformedJson_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid json", Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Malformed JSON should be rejected");
    }

    /// <summary>
    /// Verify that null email in login is rejected.
    /// </summary>
    [Fact]
    public async Task NullEmail_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"password\":\"test\"}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that empty email in login is rejected.
    /// </summary>
    [Fact]
    public async Task EmptyEmail_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"\",\"password\":\"test\"}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that null password in login is rejected.
    /// </summary>
    [Fact]
    public async Task NullPassword_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\"}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that empty password in login is rejected.
    /// </summary>
    [Fact]
    public async Task EmptyPassword_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\",\"password\":\"\"}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that invalid email format is rejected.
    /// </summary>
    [Fact]
    public async Task InvalidEmailFormat_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"not-an-email\",\"password\":\"test123\"}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that extremely long email is rejected (prevent DoS via large inputs).
    /// </summary>
    [Fact]
    public async Task ExtremelyLongEmail_IsRejected()
    {
        // Arrange
        var longEmail = new string('a', 10000) + "@example.com";

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent($"{{\"email\":\"{longEmail}\",\"password\":\"test123\"}}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            "Extremely long email should be rejected");
    }

    /// <summary>
    /// Verify that extremely long password is rejected (prevent DoS).
    /// </summary>
    [Fact]
    public async Task ExtremelyLongPassword_IsRejected()
    {
        // Arrange
        var longPassword = new string('a', 10000);

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent($"{{\"email\":\"test@example.com\",\"password\":\"{longPassword}\"}}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            "Extremely long password should be rejected");
    }

    /// <summary>
    /// Verify that missing Content-Type header is properly handled.
    /// </summary>
    [Fact]
    public async Task MissingContentType_IsHandled()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = new StringContent("{\"email\":\"test@example.com\",\"password\":\"test\"}")
        };
        request.Content.Headers.Clear();

        var response = await _client.SendAsync(request);

        // Assert - should be rejected or handled gracefully
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.UnsupportedMediaType,
            "Missing Content-Type should be rejected");
    }

    /// <summary>
    /// Verify that invalid Content-Type is rejected.
    /// </summary>
    [Fact]
    public async Task InvalidContentType_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\",\"password\":\"test\"}", 
                Encoding.UTF8, "text/plain"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.UnsupportedMediaType,
            "Invalid Content-Type should be rejected");
    }

    /// <summary>
    /// Verify that SQL injection patterns in email are rejected/escaped.
    /// </summary>
    [Fact]
    public async Task SqlInjectionPattern_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"admin'--\",\"password\":\"test123\"}", Encoding.UTF8, "application/json"));

        // Assert - should be rejected as invalid email format
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that HTML/script injection in email is rejected.
    /// </summary>
    [Fact]
    public async Task HtmlScriptInjection_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"<script>alert('xss')</script>@example.com\",\"password\":\"test123\"}", 
                Encoding.UTF8, "application/json"));

        // Assert - should be rejected as invalid email format
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that special Unicode characters in email are handled safely.
    /// </summary>
    [Fact]
    public async Task UnicodeCharactersInEmail_AreHandledSafely()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"тест@example.com\",\"password\":\"test123\"}", Encoding.UTF8, "application/json"));

        // Assert - should be rejected or handled safely (depends on validation rules)
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized,
            "Unicode in email should be handled safely");
    }

    /// <summary>
    /// Verify that null byte injection is rejected.
    /// </summary>
    [Fact]
    public async Task NullByteInjection_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test\\u0000@example.com\",\"password\":\"test123\"}", Encoding.UTF8, "application/json"));

        // Assert - should be rejected
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized,
            "Null bytes should be rejected");
    }

    /// <summary>
    /// Verify that duplicate parameters are handled correctly.
    /// </summary>
    [Fact]
    public async Task DuplicateJsonProperties_AreHandledCorrectly()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\",\"email\":\"attacker@example.com\",\"password\":\"test123\"}", 
                Encoding.UTF8, "application/json"));

        // Assert - JSON parser should use first or last occurrence, not both
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that query string parameters are not accepted where body is expected.
    /// </summary>
    [Fact]
    public async Task QueryStringParameters_AreNotAcceptedForAuth()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login?email=test@example.com&password=test123",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        // Assert - should be rejected or ignored
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that extra unknown properties don't break parsing.
    /// </summary>
    [Fact]
    public async Task ExtraUnknownProperties_AreSafelyIgnored()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\",\"password\":\"test123\",\"admin\":true,\"role\":\"superuser\"}", 
                Encoding.UTF8, "application/json"));

        // Assert - extra properties should be ignored, not used
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Still fails auth, but doesn't elevate privileges
    }

    /// <summary>
    /// Verify that numeric values in string fields are rejected.
    /// </summary>
    [Fact]
    public async Task NumericValuesInStringFields_AreRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":123,\"password\":456}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Numeric values in string fields should be rejected");
    }

    /// <summary>
    /// Verify that boolean values in string fields are rejected.
    /// </summary>
    [Fact]
    public async Task BooleanValuesInStringFields_AreRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":true,\"password\":false}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Boolean values in string fields should be rejected");
    }

    /// <summary>
    /// Verify that null values in required fields are rejected.
    /// </summary>
    [Fact]
    public async Task NullValuesInRequiredFields_AreRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":null,\"password\":null}", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verify that array values in string fields are rejected.
    /// </summary>
    [Fact]
    public async Task ArrayValuesInStringFields_AreRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":[\"test@example.com\"],\"password\":[\"test123\"]}", 
                Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Array values in string fields should be rejected");
    }

    /// <summary>
    /// Verify that object values in string fields are rejected.
    /// </summary>
    [Fact]
    public async Task ObjectValuesInStringFields_AreRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":{\"value\":\"test@example.com\"},\"password\":{\"value\":\"test123\"}}", 
                Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Object values in string fields should be rejected");
    }

    /// <summary>
    /// Verify that deeply nested JSON structures are rejected or limited.
    /// </summary>
    [Fact]
    public async Task DeeplyNestedJson_IsLimitedOrRejected()
    {
        // Arrange - create deeply nested structure
        var nested = "{\"a\":" + string.Concat(Enumerable.Range(0, 100).Select(_ => "{\"a\":")) + "1" + 
                     string.Concat(Enumerable.Range(0, 100).Select(_ => "}")) + "}";

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent(nested, Encoding.UTF8, "application/json"));

        // Assert - should be rejected
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
            "Deeply nested JSON should be rejected or limited");
    }
}
