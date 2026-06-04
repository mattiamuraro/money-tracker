using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

public class RequestObservabilityMiddlewareTests
{
    private sealed class FakeLogger : ILogger<RequestObservabilityMiddleware>
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

    private static RequestDelegate NextReturnsCompleted() => _ => Task.CompletedTask;

    private static (RequestObservabilityMiddleware middleware, FakeLogger logger) CreateMiddleware(
        RequestDelegate? next = null)
    {
        var logger = new FakeLogger();
        var middleware = new RequestObservabilityMiddleware(next ?? NextReturnsCompleted());
        return (middleware, logger);
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        var called = false;
        RequestDelegate next = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var (middleware, logger) = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, logger);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_Should_Set_Activity_Tags_For_Request_And_Response()
    {
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/";
        context.Response.StatusCode = StatusCodes.Status202Accepted;
        context.Items["CorrelationId"] = "corr-123";

        using var activity = new Activity("request").Start();

        await middleware.InvokeAsync(context, logger);

        Assert.Equal("corr-123", activity.GetTagItem("correlation.id"));
        Assert.Equal("GET", activity.GetTagItem("http.method"));
        Assert.Equal("/", activity.GetTagItem("http.route"));
        Assert.Equal(202, activity.GetTagItem("http.status_code"));
        Assert.NotNull(activity.GetTagItem("http.response.elapsed_ms"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Request_Completion()
    {
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, logger);

        Assert.Contains(logger.Entries, entry =>
            entry.Level == LogLevel.Information
            && entry.Message.Contains("Request completed with status code", StringComparison.Ordinal));
    }

    [Fact]
    public void UseRequestObservability_Should_Return_IApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        var result = RequestObservabilityMiddlewareExtensions.UseRequestObservability(app);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IApplicationBuilder>(result);
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw_When_No_Activity_Is_Active()
    {
        // Ensure Activity.Current is null — no ambient activity
        Activity.Current = null;

        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/test";

        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, logger));

        Assert.Null(exception);
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw_When_CorrelationId_Is_Absent_From_Context_Items()
    {
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        // Deliberately omit CorrelationId from context.Items

        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, logger));

        Assert.Null(exception);
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_StatusCode_In_Completion_Message()
    {
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Response.StatusCode = StatusCodes.Status404NotFound;

        await middleware.InvokeAsync(context, logger);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information
            && e.Message.Contains("404", StringComparison.Ordinal));
    }
}
