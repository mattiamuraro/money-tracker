using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

public class UserScopeMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Set_Activity_Tags_For_Authenticated_User()
    {
        var middleware = new UserScopeMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "john.doe")
        ],
        authenticationType: "test"));

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserScopeMiddleware>();

        using var activity = new Activity("request").Start();

        await middleware.InvokeAsync(context, logger);

        Assert.NotNull(activity.GetTagItem("user.id"));
        Assert.NotNull(activity.GetTagItem("enduser.id"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Set_User_Tags_For_Anonymous_User()
    {
        var middleware = new UserScopeMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserScopeMiddleware>();

        using var activity = new Activity("request").Start();

        await middleware.InvokeAsync(context, logger);

        Assert.Null(activity.GetTagItem("user.id"));
        Assert.Null(activity.GetTagItem("enduser.id"));
    }

    [Fact]
    public void UseUserScope_Should_Return_IApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        var result = UserScopeMiddlewareExtensions.UseUserScope(app);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IApplicationBuilder>(result);
    }
}
