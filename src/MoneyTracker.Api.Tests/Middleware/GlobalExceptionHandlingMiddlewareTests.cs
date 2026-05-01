using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandlingMiddleware
/// </summary>
public class GlobalExceptionHandlingMiddlewareTests
{
    private sealed class FakeLogger : ILogger<GlobalExceptionHandlingMiddleware>
    {
        public record LogEntry(LogLevel Level, Exception? Exception, string Message);
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "TestApp";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static (GlobalExceptionHandlingMiddleware middleware, FakeLogger logger) CreateMiddleware(
        RequestDelegate? next = null,
        IWebHostEnvironment? environment = null)
    {
        var logger = new FakeLogger();
        var env = environment ?? new FakeWebHostEnvironment();
        var middleware = new GlobalExceptionHandlingMiddleware(next ?? (_ => Task.CompletedTask), logger, env);
        return (middleware, logger);
    }

    [Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        var (middleware, _) = CreateMiddleware();
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate_When_No_Exception()
    {
        // Arrange
        var called = false;
        var (middleware, _) = CreateMiddleware(_ => { called = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_Should_Handle_Exception_When_Next_Throws()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var (middleware, logger) = CreateMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Error &&
            e.Exception == exception &&
            e.Message.Contains("An unhandled exception occurred"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Set_Response_ContentType_When_Exception_Occurs()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("Test exception"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw_When_Next_Throws_Exception()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("Test exception"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act & Assert - should not throw
        await middleware.InvokeAsync(context);
    }
}
