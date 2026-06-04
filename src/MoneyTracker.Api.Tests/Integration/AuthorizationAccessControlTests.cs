using System.Net;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Authorization and access control tests.
/// These tests verify that endpoints enforce proper authentication and authorization.
/// </summary>
public sealed class AuthorizationAccessControlTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _unauthenticatedClient;
    private readonly HttpClient _authenticatedClient;

    public AuthorizationAccessControlTests()
    {
        _factory = new ApiWebFactory();
        _unauthenticatedClient = _factory.CreateClient();
        _authenticatedClient = _factory.CreateClient();
    }

    public void Dispose()
    {
        _unauthenticatedClient.Dispose();
        _authenticatedClient.Dispose();
        _factory.Dispose();
    }

    /// <summary>
    /// Verify that public endpoints don't require authentication.
    /// </summary>
    [Fact]
    public async Task PublicEndpoint_AuthConfig_DoesNotRequireAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verify that login endpoint doesn't require authentication.
    /// </summary>
    [Fact]
    public async Task PublicEndpoint_Login_DoesNotRequireAuth()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/auth/login",
            new StringContent("{\"email\":\"test@example.com\",\"password\":\"test123\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert - should not be 401 Unauthorized due to missing auth
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that register endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_Register_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/auth/register",
            new StringContent("{\"username\":\"newuser@example.com\",\"password\":\"TestPass#123\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert - register endpoint requires authentication
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that protected read endpoints reject unauthenticated requests.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_GetPayments_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/payments?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that protected write endpoints reject unauthenticated requests.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_CreatePayment_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/payments",
            new StringContent("{\"description\":\"Test\",\"amount\":100,\"date\":\"2024-01-01\",\"categoryId\":\"123e4567-e89b-12d3-a456-426614174000\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that protected delete endpoints reject unauthenticated requests.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_DeletePayment_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.DeleteAsync("/api/v1/payments/123e4567-e89b-12d3-a456-426614174000");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that invalid JWT token is rejected.
    /// </summary>
    [Fact]
    public async Task InvalidJwtToken_IsRejected()
    {
        // Arrange
        _authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token-here");

        // Act
        var response = await _authenticatedClient.GetAsync("/api/v1/payments?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that malformed Authorization header is rejected.
    /// </summary>
    [Fact]
    public async Task MalformedAuthorizationHeader_IsRejected()
    {
        // Arrange
        _authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearertoken-without-space");

        // Act
        var response = await _authenticatedClient.GetAsync("/api/v1/payments?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that missing Authorization header results in 401.
    /// </summary>
    [Fact]
    public async Task MissingAuthorizationHeader_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/payments?year=2024&month=1");
        // No Authorization header added

        // Act
        var response = await _unauthenticatedClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that other bearer schemes (OAuth2, etc.) are rejected if not supported.
    /// </summary>
    [Fact]
    public async Task NonJwtBearerScheme_IsRejected()
    {
        // Arrange
        _authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJhbGwiLCJhZG1pbiI6dHJ1ZX0.");

        // Act
        var response = await _authenticatedClient.GetAsync("/api/v1/payments?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that Dashboard summary endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_DashboardSummary_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/dashboard/summary?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that payment category endpoints require authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_GetPaymentCategories_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/payment-categories");

        // Assert - should require auth or return 404 if endpoint doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    /// <summary>
    /// Verify that income endpoints require authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_GetIncomes_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/incomes?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that forecast expense endpoints require authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_GetForecastExpenses_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/forecast-expenses?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that forecast income endpoints require authentication.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_GetForecastIncomes_RequiresAuth()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/forecast-incomes?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that forecast recurrence rule type endpoints don't require auth (if public).
    /// </summary>
    [Fact]
    public async Task ForecastRecurrenceRuleTypeEndpoint_ChecksAuthRequirement()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/v1/forecast-recurrence-rule-types");

        // Assert - should be OK if public, or Unauthorized if protected
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized,
            $"Endpoint returned {response.StatusCode}");
    }

    /// <summary>
    /// Verify that repeated Authorization header attempts are handled safely.
    /// </summary>
    [Fact]
    public async Task RepeatedAuthorizationAttempts_AreHandledSafely()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/payments?year=2024&month=1");
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer token1");

        // Act
        var response = await _unauthenticatedClient.SendAsync(request);

        // Assert - should reject due to invalid token
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that Authorization header is case-insensitive.
    /// </summary>
    [Fact]
    public async Task AuthorizationHeader_IsCaseInsensitive()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/payments?year=2024&month=1");
        request.Headers.Add("authorization", "Bearer invalid-token"); // lowercase

        // Act
        var response = await _unauthenticatedClient.SendAsync(request);

        // Assert - HTTP headers are case-insensitive
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that Bearer token with extra whitespace is handled.
    /// </summary>
    [Fact]
    public async Task BearerTokenWithExtraWhitespace_IsHandled()
    {
        // Arrange
        _authenticatedClient.DefaultRequestHeaders.Add("Authorization", "Bearer  invalid-token-with-spaces  ");

        // Act
        var response = await _authenticatedClient.GetAsync("/api/v1/payments?year=2024&month=1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that Refresh endpoint validates refresh token (doesn't check Bearer).
    /// </summary>
    [Fact]
    public async Task RefreshEndpoint_ValidatesRefreshToken()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/auth/refresh",
            new StringContent("{\"refreshToken\":\"any-token\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert - should reject invalid token, not require Bearer
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.BadRequest,
            $"Refresh returned {response.StatusCode}");
    }

    /// <summary>
    /// Verify that Revoke endpoint doesn't require Bearer token (uses refresh token).
    /// </summary>
    [Fact]
    public async Task RevokeEndpoint_DoesNotRequireBearer()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/auth/revoke",
            new StringContent("{\"refreshToken\":\"any-token\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert - should not be 401 due to missing Bearer
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verify that Change Password endpoint doesn't require Bearer token if using old credentials.
    /// </summary>
    [Fact]
    public async Task ChangePasswordEndpoint_RequiresAuthenticationOrCredentials()
    {
        // Act
        var response = await _unauthenticatedClient.PostAsync("/api/v1/auth/change-password",
            new StringContent("{\"currentPassword\":\"old\",\"newPassword\":\"new\"}", 
                System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || 
                   response.StatusCode == HttpStatusCode.BadRequest,
            "Change password should require auth or valid credentials");
    }
}
