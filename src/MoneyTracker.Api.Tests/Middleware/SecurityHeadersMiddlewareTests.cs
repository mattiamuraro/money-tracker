using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AddsSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        // Assert
        Assert.True(context.Response.Headers.TryGetValue("X-Content-Type-Options", out StringValues noSniff));
        Assert.Equal("nosniff", noSniff.ToString());

        Assert.True(context.Response.Headers.TryGetValue("X-Frame-Options", out StringValues frameOptions));
        Assert.Equal("DENY", frameOptions.ToString());

        Assert.True(context.Response.Headers.TryGetValue("Referrer-Policy", out StringValues referrerPolicy));
        Assert.Equal("no-referrer", referrerPolicy.ToString());

        Assert.True(context.Response.Headers.TryGetValue("Permissions-Policy", out StringValues permissionsPolicy));
        Assert.Equal("camera=(), microphone=(), geolocation=()", permissionsPolicy.ToString());

        Assert.True(context.Response.Headers.TryGetValue("Content-Security-Policy", out StringValues csp));
        Assert.Equal("default-src 'none'; frame-ancestors 'none'; base-uri 'none'", csp.ToString());
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_Delegate()
    {
        // Arrange
        var called = false;
        var middleware = new SecurityHeadersMiddleware(_ => { called = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotOverwrite_ExistingSecurityHeader()
    {
        // Arrange – a previous middleware already set X-Frame-Options to SAMEORIGIN
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

        // Act
        await middleware.InvokeAsync(context);

        // Assert – TryAdd must not replace the pre-existing value
        Assert.True(context.Response.Headers.TryGetValue("X-Frame-Options", out StringValues value));
        Assert.Equal("SAMEORIGIN", value.ToString());
    }
}
