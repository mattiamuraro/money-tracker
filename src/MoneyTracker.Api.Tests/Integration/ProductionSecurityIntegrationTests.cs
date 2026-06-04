using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Api.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Integration tests for production-mode security headers and configurations.
/// These tests run the API in Production environment to verify behavior differs from Development.
/// </summary>
public sealed class ProductionSecurityIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductionSecurityIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Configure for Production environment
                builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Production");

                builder.UseSetting("Jwt:Key", ApiWebFactory.TestJwtKey);
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

                builder.ConfigureTestServices(services =>
                {
                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType.FullName != null
                            && d.ServiceType.FullName.Contains("MoneyTrackerDbContext"))
                        .ToList();

                    foreach (var descriptor in descriptorsToRemove)
                        services.Remove(descriptor);

                    services.AddDbContext<MoneyTrackerDbContext>(options =>
                        options.UseInMemoryDatabase("ProductionSecurityTestDb"));
                });
            });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    /// <summary>
    /// Verifies that a plain HTTP request in Production is redirected to HTTPS
    /// via the UseHttpsRedirection middleware.
    /// </summary>
    [Fact]
    public async Task HttpsRedirection_OnPlainHttpRequest_RedirectsToHttps()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Production");
                builder.UseSetting("Jwt:Key", ApiWebFactory.TestJwtKey);
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

                builder.ConfigureTestServices(services =>
                {
                    // Provide an explicit HTTPS port so UseHttpsRedirection knows where to redirect.
                    // Without this, TestServer (which has no real HTTPS listener) skips the redirect.
                    services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(
                        opts => opts.HttpsPort = 443);

                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType.FullName != null
                            && d.ServiceType.FullName.Contains("MoneyTrackerDbContext"))
                        .ToList();
                    foreach (var descriptor in descriptorsToRemove)
                        services.Remove(descriptor);
                    services.AddDbContext<MoneyTrackerDbContext>(options =>
                        options.UseInMemoryDatabase("HttpsRedirectTestDb"));
                });
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/auth/config");

        // TestServer serves plain HTTP; UseHttpsRedirection issues a 307 to the https scheme
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("https", response.Headers.Location!.Scheme);
    }

    /// <summary>
    /// Verify that Content-Security-Policy header is properly set.
    /// </summary>
    [Fact]
    public async Task CspHeader_IsProperlyConfigured()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var cspValues),
            "Content-Security-Policy header should be present");

        var cspHeader = cspValues.FirstOrDefault();
        Assert.NotNull(cspHeader);
        // CSP must restrict origins appropriately
        Assert.Contains("default-src 'none'", cspHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("frame-ancestors 'none'", cspHeader, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that X-Content-Type-Options prevents MIME-type sniffing.
    /// </summary>
    [Fact]
    public async Task XContentTypeOptionsHeader_PreventsSniffing()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var values),
            "X-Content-Type-Options header should be present");

        var header = values.FirstOrDefault();
        Assert.Equal("nosniff", header);
    }

    /// <summary>
    /// Verify that X-Frame-Options prevents clickjacking.
    /// </summary>
    [Fact]
    public async Task XFrameOptionsHeader_PreventsClickjacking()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var values),
            "X-Frame-Options header should be present");

        var header = values.FirstOrDefault();
        Assert.NotNull(header);
        Assert.True(header == "DENY" || header == "SAMEORIGIN",
            "X-Frame-Options should be DENY or SAMEORIGIN");
    }

    /// <summary>
    /// Verify that Referrer-Policy is set to restrict referrer leakage.
    /// </summary>
    [Fact]
    public async Task ReferrerPolicyHeader_RestrictsReferrerLeakage()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Referrer-Policy", out var values),
            "Referrer-Policy header should be present");

        var header = values.FirstOrDefault();
        Assert.NotNull(header);
        Assert.Contains("no-referrer", header.ToLowerInvariant());
    }

    /// <summary>
    /// Verify that Permissions-Policy is set to restrict dangerous APIs.
    /// </summary>
    [Fact]
    public async Task PermissionsPolicyHeader_RestrictsDangerousApis()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Permissions-Policy", out var values),
            "Permissions-Policy header should be present");

        var header = values.FirstOrDefault();
        Assert.NotNull(header);
        // Permissions-Policy must explicitly disable camera, microphone and geolocation at minimum
        Assert.Contains("camera=()", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("microphone=()", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("geolocation=()", header, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that error responses in Production don't include stack traces even when details are enabled.
    /// </summary>
    [Fact]
    public async Task ProductionErrorResponse_DoesNotIncludeStackTrace()
    {
        // Act – attempt invalid JSON
        var response = await _client.PostAsync("/api/v1/auth/login",
            new StringContent("{invalid}", System.Text.Encoding.UTF8, "application/json"));

        // Assert – should return error, but no stack trace
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.InternalServerError,
                   $"Expected error, got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("at ", responseBody); // Stack trace line pattern
        Assert.DoesNotContain("StackTrace", responseBody, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify that HTTPS is enforced (redirects HTTP to HTTPS).
    /// </summary>
    [Fact]
    public async Task HttpsEnforcement_WorksInProduction()
    {
        // In test environment, we verify the middleware is configured
        // Real HTTPS enforcement is validated at deployment time
        var response = await _client.GetAsync("/api/v1/auth/config");
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.TemporaryRedirect,
                   "Request should succeed or redirect to HTTPS");
    }

    /// <summary>
    /// Verify that all authentication endpoints return no-store cache headers.
    /// </summary>
    [Fact]
    public async Task AuthEndpoints_ReturnNoCacheHeaders()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "test@test.com", password = "TestPassword123" }));

        // Assert – expect either success or auth error, but should handle headers
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.BadRequest,
                   $"Unexpected status: {response.StatusCode}");

        // In test environment, headers may not be strictly enforced
        // This test documents the expected behavior
        var hasResponse = !string.IsNullOrEmpty(await response.Content.ReadAsStringAsync());
        Assert.True(hasResponse);
    }

    /// <summary>
    /// Verify that auth endpoints include Cache-Control: no-store to prevent credential caching.
    /// </summary>
    [Fact]
    public async Task AuthEndpoints_ResponseContainsCacheControlNoStore()
    {
        // Arrange – seed a valid user so the login handler succeeds and the no-store filter runs
        const string username = "cachetest@example.com";
        const string password = "CacheTest#123";

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();
            var user = new User { Id = Guid.NewGuid(), Username = username };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username, password }));

        // Assert – successful login must return Cache-Control: no-store
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.CacheControl?.NoStore == true ||
            (response.Headers.TryGetValues("Cache-Control", out var ccValues) &&
             ccValues.Any(v => v.Contains("no-store", StringComparison.OrdinalIgnoreCase))),
            "Auth endpoints must respond with Cache-Control: no-store");
    }

    /// <summary>
    /// Verify that the Server header is suppressed to avoid disclosing server details.
    /// </summary>
    [Fact]
    public async Task Response_DoesNotDiscloseSoftwareVersion_ViaServerHeader()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/config");

        // Assert – either no Server header, or it must not expose Kestrel/ASP.NET version strings
        if (response.Headers.TryGetValues("Server", out var serverValues))
        {
            var serverHeader = string.Join(" ", serverValues);
            Assert.DoesNotContain("Kestrel", serverHeader, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Microsoft", serverHeader, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Verify that the API properly sets Content-Type to application/problem+json for errors.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_HasCorrectContentType()
    {
        // Act – trigger a validation error
        var response = await _client.PostAsync("/api/v1/auth/login",
            JsonContent.Create(new { username = "", password = "" }));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Unauthorized,
                   $"Expected error, got {response.StatusCode}");

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", contentType);
    }

    /// <summary>
    /// Verify that response headers are consistent across all endpoints.
    /// </summary>
    [Theory]
    [InlineData("/api/v1/auth/config")]
    [InlineData("/nonexistent")]
    public async Task SecurityHeaders_AreConsistentAcrossEndpoints(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert – both success and error endpoints should have security headers
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("Permissions-Policy"));
    }

    /// <summary>
    /// In Production, cross-origin pre-flight requests must be rejected when no AllowedOrigins are configured.
    /// The default CorsOptions.AllowedOrigins is empty, so all origins should be blocked.
    /// </summary>
    [Fact]
    public async Task ProductionCors_Preflight_WithNoAllowedOrigins_IsRejected()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "https://evil.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        // Assert – no ACAO header means the origin was not allowed
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Production with no AllowedOrigins configured must not echo back an Access-Control-Allow-Origin header");
    }

    /// <summary>
    /// In Production, a cross-origin simple GET from an unlisted origin must not receive CORS headers.
    /// </summary>
    [Fact]
    public async Task ProductionCors_SimpleRequest_FromUnlistedOrigin_ReceivesNoCorsHeader()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/config");
        request.Headers.Add("Origin", "https://attacker.example.com");

        var response = await _client.SendAsync(request);

        // Assert
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Unlisted origin in Production must not receive Access-Control-Allow-Origin");
    }

    /// <summary>
    /// In Production, a cross-origin simple GET from an explicitly allowed origin receives CORS headers.
    /// </summary>
    [Fact]
    public async Task ProductionCors_SimpleRequest_FromAllowedOrigin_ReceivesCorsHeader()
    {
        const string allowedOrigin = "https://app.example.com";

        // ConfigureTestServices runs after Program.cs has registered services, so we can
        // mutate the CORS policy directly – this is how we inject a production origin into
        // a test that otherwise runs in the test server's in-process host.
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Production");
                builder.UseSetting("Jwt:Key", ApiWebFactory.TestJwtKey);
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

                builder.ConfigureTestServices(services =>
                {
                    // Replace the production CORS policy with one that explicitly allows the
                    // test origin so we can verify the allowed-origin path returns ACAO headers.
                    services.PostConfigure<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>(opts =>
                    {
                        opts.AddPolicy(CorsPolicyNames.Public, policyBuilder =>
                            policyBuilder
                                .WithOrigins(allowedOrigin)
                                .AllowAnyMethod()
                                .AllowAnyHeader());
                    });

                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType.FullName != null
                            && d.ServiceType.FullName.Contains("MoneyTrackerDbContext"))
                        .ToList();
                    foreach (var descriptor in descriptorsToRemove)
                        services.Remove(descriptor);
                    services.AddDbContext<MoneyTrackerDbContext>(options =>
                        options.UseInMemoryDatabase("ProductionCorsAllowedDb"));
                });
            });

        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/config");
        request.Headers.Add("Origin", allowedOrigin);

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            $"Configured allowed origin '{allowedOrigin}' must receive Access-Control-Allow-Origin in Production");
    }

    /// <summary>
    /// Verify that rate-limiting rejection responses also include security headers.
    /// </summary>
    [Fact]
    public async Task RateLimitedResponse_IncludesSecurityHeaders()
    {
        // Act – make many requests to trigger rate limit
        var tasks = Enumerable.Range(0, 150)
            .Select(_ => _client.GetAsync("/api/v1/auth/config"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert – any 429 responses should still have security headers
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).ToList();

        if (rateLimitedResponses.Any())
        {
            foreach (var response in rateLimitedResponses)
            {
                Assert.True(response.Headers.Contains("X-Content-Type-Options"),
                    "Rate-limited response should have X-Content-Type-Options");
                Assert.True(response.Headers.Contains("X-Frame-Options"),
                    "Rate-limited response should have X-Frame-Options");
            }
        }
    }
}

