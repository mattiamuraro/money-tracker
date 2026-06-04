using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Spins up the real API host using TestServer with the EF SQL dependency replaced by an in-memory store
/// and a valid minimal configuration so all startup option validators pass.
/// </summary>
public class ApiWebFactory : WebApplicationFactory<Program>
{
    // A 32-byte key so the JWT key-length validator is satisfied.
    internal const string TestJwtKey = "integration-test-secret-key-32bytes!!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Supply the minimum config values required by ValidateOnStart option validators.
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", "MoneyTracker.Api.Tests");
        builder.UseSetting("Jwt:Audience", "MoneyTracker.Tests");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.UseSetting("Auth:RefreshToken:ExpiryDays", "7");
        builder.UseSetting("Auth:PasswordPolicy:MinimumLength", "12");
        builder.UseSetting("Auth:PasswordPolicy:MaximumLength", "128");
        builder.UseSetting("Auth:PasswordPolicy:PasswordHistoryCount", "5");
        builder.UseSetting("Security:LoginProtection:MaxFailedAttempts", "5");
        builder.UseSetting("Security:LoginProtection:LockoutMinutes", "15");
        builder.UseSetting("Security:RequestLimits:MaxRequestBodySizeBytes", "1048576");
        builder.UseSetting("Security:RequestLimits:MaxRequestHeadersTotalSizeBytes", "32768");
        builder.UseSetting("ReverseProxy:ForwardLimit", "2");
        // Development environment keeps CORS open and skips Azure Key Vault.
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Aspire registers a singleton DbContextPool; remove all EF registrations for this context
            // so the in-memory replacement doesn't conflict with the pool's lifetime expectations.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null
                    && d.ServiceType.FullName.Contains("MoneyTrackerDbContext"))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<MoneyTrackerDbContext>(options =>
                options.UseInMemoryDatabase("ApiIntegrationTestDb"));
        });
    }

    /// <summary>
    /// Seed a test user with a known password and return the user ID.
    /// </summary>
    internal async Task<Guid> SeedTestUserAsync(HttpClient client, string username = "testuser@example.com", string password = "TestPass#123")
    {
        // Use the ServiceProvider to access the DbContext and create a user.
        using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username
        };

        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }
}
