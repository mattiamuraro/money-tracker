using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoneyTracker.Api.Middleware;
using System.Text;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for RequestResponseLoggingMiddleware
/// </summary>
public class RequestResponseLoggingMiddlewareTests
{
    private sealed class FakeLogger : ILogger<RequestResponseLoggingMiddleware>
    {
        public record LogEntry(LogLevel Level, string Message);
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private static (RequestResponseLoggingMiddleware middleware, FakeLogger logger) CreateMiddleware(
        RequestDelegate? next = null)
    {
        var logger = new FakeLogger();
        var middleware = new RequestResponseLoggingMiddleware(next ?? (_ => Task.CompletedTask), logger);
        return (middleware, logger);
    }

    private static HttpContext CreateHttpContext(
        string method,
        string path,
        string queryString,
        string? requestBody = null,
        string? contentType = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(queryString);

        if (requestBody != null)
        {
            var bytes = Encoding.UTF8.GetBytes(requestBody);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
        }

        if (contentType != null)
        {
            context.Request.ContentType = contentType;
        }

        context.Response.Body = new MemoryStream();

        return context;
    }

    [Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        var (middleware, _) = CreateMiddleware();
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Request_Information()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("GET", "/api/test", "?foo=bar");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Request:") &&
            e.Message.Contains("GET") &&
            e.Message.Contains("/api/test"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Response_Information()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware(ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext("POST", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Response:") &&
            e.Message.Contains("200") &&
            e.Message.Contains("completed in"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Request_Body_When_Json_Content_Type()
    {
        // Arrange
        var requestBody = "{\"test\":\"value\"}";
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Debug &&
            e.Message.Contains("Request Body:") &&
            e.Message.Contains(requestBody));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Log_Request_Body_When_Not_Json_Content_Type()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("POST", "/api/test", string.Empty, "plain text body", "text/plain");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Request Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Log_Request_Body_When_Empty()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Request Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Response_Body_When_Json_Content_Type()
    {
        // Arrange
        var responseBody = "{\"result\":\"success\"}";
        var (middleware, logger) = CreateMiddleware(ctx =>
        {
            ctx.Response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            return ctx.Response.Body.WriteAsync(bytes).AsTask();
        });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Debug &&
            e.Message.Contains("Response Body:") &&
            e.Message.Contains(responseBody));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Log_Response_Body_When_Not_Json_Content_Type()
    {
        // Arrange
        var responseBody = "plain text response";
        var (middleware, logger) = CreateMiddleware(ctx =>
        {
            ctx.Response.ContentType = "text/plain";
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            return ctx.Response.Body.WriteAsync(bytes).AsTask();
        });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Response Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Log_Response_Body_When_Empty()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware(ctx =>
        {
            ctx.Response.ContentType = "application/json";
            return Task.CompletedTask;
        });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Response Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        // Arrange
        var called = false;
        var (middleware, _) = CreateMiddleware(_ => { called = true; return Task.CompletedTask; });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_Should_Copy_Response_To_Original_Stream()
    {
        // Arrange
        var responseBody = "test response";
        var (middleware, _) = CreateMiddleware(ctx =>
        {
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            return ctx.Response.Body.WriteAsync(bytes).AsTask();
        });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);
        var originalStream = new MemoryStream();
        context.Response.Body = originalStream;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        originalStream.Seek(0, SeekOrigin.Begin);
        var content = await new StreamReader(originalStream).ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Equal(responseBody, content);
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Elapsed_Time()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware(async _ => await Task.Delay(50));
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("completed in") &&
            e.Message.Contains("ms"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Handle_Null_Request_ContentType()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("POST", "/api/test", string.Empty, "{\"test\":\"value\"}", null);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Request Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Handle_Null_Response_ContentType()
    {
        // Arrange
        var responseBody = "{\"result\":\"success\"}";
        var (middleware, logger) = CreateMiddleware(ctx =>
        {
            ctx.Response.ContentType = null;
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            return ctx.Response.Body.WriteAsync(bytes).AsTask();
        });
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Debug && e.Message.Contains("Response Body:"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_QueryString_When_Present()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("GET", "/api/test", "?param1=value1&param2=value2");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("?param1=value1&param2=value2"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Handle_Json_ContentType_With_Charset()
    {
        // Arrange
        var requestBody = "{\"test\":\"value\"}";
        var (middleware, logger) = CreateMiddleware();
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, "application/json; charset=utf-8");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Debug &&
            e.Message.Contains("Request Body:") &&
            e.Message.Contains(requestBody));
    }
}
