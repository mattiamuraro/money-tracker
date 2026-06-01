using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Integration tests for the refresh-token security lifecycle:
/// one-time-use rotation (reuse of a consumed token must be rejected) and
/// revocation (a revoked token must not be accepted for refresh).
/// </summary>
public sealed class RefreshTokenLifecycleIntegrationTests : IAsyncLifetime
{
    private readonly ApiWebFactory _factory;
    private HttpClient _client = null!;
    private const string Username = "refreshtest@example.com";
    private const string Password = "RefreshTest#123";

    public RefreshTokenLifecycleIntegrationTests()
    {
        _factory = new ApiWebFactory();
    }

    public async ValueTask InitializeAsync()
    {
        _client = _factory.CreateClient();
        await _factory.SeedTestUserAsync(_client, Username, Password);
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return ValueTask.CompletedTask;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<(string accessToken, string refreshToken)> LoginAsync()
    {
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = Username, password = Password }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        return (root.GetProperty("token").GetString()!, root.GetProperty("refreshToken").GetString()!);
    }

    private async Task<string?> TryRefreshAsync(string refreshToken)
    {
        var response = await _client.PostAsync("/api/v1/auth/refresh",
            JsonContent.Create(new { refreshToken }));

        if (!response.IsSuccessStatusCode)
            return null;

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("refreshToken").GetString();
    }

    // ── one-time-use rotation ─────────────────────────────────────────────────

    /// <summary>
    /// After a token is rotated (consumed via refresh), replaying the original token must be rejected.
    /// </summary>
    [Fact]
    public async Task RefreshToken_AfterRotation_OldTokenIsRejected()
    {
        // Arrange – obtain an initial refresh token
        var (_, originalRefreshToken) = await LoginAsync();

        // Act – consume the original token once (successful rotation)
        var rotatedToken = await TryRefreshAsync(originalRefreshToken);
        Assert.NotNull(rotatedToken); // first use must succeed

        // Act – replay the now-consumed original token
        var replayResponse = await _client.PostAsync("/api/v1/auth/refresh",
            JsonContent.Create(new { refreshToken = originalRefreshToken }));

        // Assert – must be rejected; the token was already consumed
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);
    }

    /// <summary>
    /// The rotated token issued during a refresh must itself be usable exactly once.
    /// </summary>
    [Fact]
    public async Task RefreshToken_RotatedToken_IsUsableOnce()
    {
        // Arrange
        var (_, originalRefreshToken) = await LoginAsync();
        var rotatedToken = await TryRefreshAsync(originalRefreshToken);
        Assert.NotNull(rotatedToken);

        // Act – use the rotated token once
        var secondRotatedToken = await TryRefreshAsync(rotatedToken);
        Assert.NotNull(secondRotatedToken); // must succeed on first use

        // Act – replay the rotated token a second time
        var replayResponse = await _client.PostAsync("/api/v1/auth/refresh",
            JsonContent.Create(new { refreshToken = rotatedToken }));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);
    }

    // ── revocation ────────────────────────────────────────────────────────────

    /// <summary>
    /// After a token is explicitly revoked, using it for refresh must be rejected.
    /// </summary>
    [Fact]
    public async Task RefreshToken_AfterRevocation_CannotBeUsedForRefresh()
    {
        // Arrange
        var (_, refreshToken) = await LoginAsync();

        // Revoke the token
        var revokeResponse = await _client.PostAsync("/api/v1/auth/revoke",
            JsonContent.Create(new { refreshToken }));
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        // Act – attempt to use the revoked token
        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh",
            JsonContent.Create(new { refreshToken }));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    /// <summary>
    /// Revoking an already-revoked token must be idempotent (no error).
    /// </summary>
    [Fact]
    public async Task RevokeToken_CalledTwice_IsIdempotent()
    {
        // Arrange
        var (_, refreshToken) = await LoginAsync();

        // Act
        var first = await _client.PostAsync("/api/v1/auth/revoke",
            JsonContent.Create(new { refreshToken }));
        var second = await _client.PostAsync("/api/v1/auth/revoke",
            JsonContent.Create(new { refreshToken }));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    /// <summary>
    /// Revoking a completely unknown token must not produce an error.
    /// </summary>
    [Fact]
    public async Task RevokeToken_WithUnknownToken_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/revoke",
            JsonContent.Create(new { refreshToken = "totally-unknown-token-value" }));

        // Assert – revoke is a best-effort operation; unknown tokens are silently ignored
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Refresh with a completely unknown token must be rejected with 401.
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithUnknownToken_IsRejected()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/refresh",
            JsonContent.Create(new { refreshToken = "totally-unknown-token-value" }));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
