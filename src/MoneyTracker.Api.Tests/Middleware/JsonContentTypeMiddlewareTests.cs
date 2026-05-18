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
}
