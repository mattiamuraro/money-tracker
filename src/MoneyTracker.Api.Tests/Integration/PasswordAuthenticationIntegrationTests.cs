using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Integration tests for password-related auth flows via the actual API host.
/// </summary>
public sealed class PasswordAuthenticationIntegrationTests : IDisposable
{
    private readonly ApiWebFactory _factory;
    private readonly HttpClient _client;

    public PasswordAuthenticationIntegrationTests()
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
    public async Task ChangePassword_WithRecentPasswordReuse_Returns400BadRequest()
    {
        // Arrange – create a user with a password history entry.
        var userId = await _factory.SeedTestUserAsync(_client, "historyuser@test.com", "Original#Password1");

        // Manually add to password history so the second change-password will fail.
        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MoneyTracker.Data.EntityFramework.MoneyTrackerDbContext>();
            var user = await db.Users.SingleAsync(u => u.Id == userId);

            // Add a history entry with the password we'll try to reuse.
            var hasher = new PasswordHasher<MoneyTracker.Data.User>();
            var reusedHash = hasher.HashPassword(user, "Reused#Password123");

            db.UserPasswordHistories.Add(new MoneyTracker.Data.UserPasswordHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PasswordHash = reusedHash,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            await db.SaveChangesAsync();
        }

        // Act – get a token by logging in (use original password).
        var loginResponse = await _client.PostAsync("/api/v1/auth/login", 
            JsonContent.Create(new { username = "historyuser@test.com", password = "Original#Password1" }));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody?.Token);

        // Set the auth header for the change-password call.
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);

        // Try to change password to a recently-used one.
        var changeResponse = await _client.PostAsync("/api/v1/auth/change-password",
            JsonContent.Create(new 
            { 
                currentPassword = "Original#Password1", 
                newPassword = "Reused#Password123" 
            }));

        // Assert – expect 400 Bad Request due to password history violation.
        Assert.Equal(HttpStatusCode.BadRequest, changeResponse.StatusCode);

        var errorBody = await changeResponse.Content.ReadAsStringAsync();
        // The error may come as a FluentValidation problem detail or a generic 400.
        // Just verify it's a 400 and that we didn't get 204 (success) or 401 (wrong current password).
        Assert.False(string.IsNullOrEmpty(errorBody));
    }

    [Fact]
    public async Task ChangePassword_WithValidNewPassword_Returns204NoContent()
    {
        // Arrange – create a user.
        var userId = await _factory.SeedTestUserAsync(_client, "validuser@test.com", "Original#Password1");

        // Act – get a token by logging in.
        var loginResponse = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "validuser@test.com", password = "Original#Password1" }));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody?.Token);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);

        // Change to a valid new password.
        var changeResponse = await _client.PostAsync("/api/v1/auth/change-password",
            JsonContent.Create(new
            {
                currentPassword = "Original#Password1",
                newPassword = "NewValid#Password123"
            }));

        // Assert – expect 204 No Content on success.
        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Returns401Unauthorized()
    {
        // Arrange – create a user.
        var userId = await _factory.SeedTestUserAsync(_client, "wrongpass@test.com", "Original#Password1");

        // Get token.
        var loginResponse = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "wrongpass@test.com", password = "Original#Password1" }));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);

        // Try to change with wrong current password.
        var changeResponse = await _client.PostAsync("/api/v1/auth/change-password",
            JsonContent.Create(new
            {
                currentPassword = "Wrong#Password123",
                newPassword = "NewValid#Password123"
            }));

        // Assert – expect 401 Unauthorized.
        Assert.Equal(HttpStatusCode.Unauthorized, changeResponse.StatusCode);
    }

    private sealed record LoginResponse(string Token, string RefreshToken);
    private sealed record ValidationProblemResponse(string Title, int Status, Dictionary<string, string[]> Errors);
}
