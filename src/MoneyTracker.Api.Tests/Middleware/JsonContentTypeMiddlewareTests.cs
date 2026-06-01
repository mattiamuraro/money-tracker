using Microsoft.AspNetCore.Http;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

public class JsonContentTypeMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PostApiRequestWithNonJsonContentType_Returns415()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/payments";
        context.Request.ContentType = "text/plain";

        var middleware = new JsonContentTypeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GetRequest_CallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/payments";

        var called = false;
        var middleware = new JsonContentTypeMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_MutatingMethodWithNonJsonContentType_Returns415(string method)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = "/api/v1/payments/1";
        context.Request.ContentType = "text/plain";
        context.Response.Body = new MemoryStream();

        var middleware = new JsonContentTypeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_PostOutsideApiPath_CallsNextMiddleware()
    {
        // Paths that do not start with /api/v1 must not be blocked
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/health";
        context.Request.ContentType = "text/plain";

        var called = false;
        var middleware = new JsonContentTypeMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Theory]
    [InlineData("application/json; charset=utf-8")]
    [InlineData("application/vnd.api+json")]
    [InlineData("application/merge-patch+json")]
    public async Task InvokeAsync_PostApiRequestWithJsonVariant_CallsNextMiddleware(string contentType)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/payments";
        context.Request.ContentType = contentType;

        var called = false;
        var middleware = new JsonContentTypeMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_PostApiRequestWithMissingContentType_Returns415()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/auth/login";
        // ContentType is null by default
        context.Response.Body = new MemoryStream();

        var middleware = new JsonContentTypeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DeleteRequest_CallsNextMiddleware()
    {
        // DELETE has no body, should never be blocked
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Delete;
        context.Request.Path = "/api/v1/payments/1";

        var called = false;
        var middleware = new JsonContentTypeMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }
}
