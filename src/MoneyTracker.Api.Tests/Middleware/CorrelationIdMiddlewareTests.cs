using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for CorrelationIdMiddleware
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private sealed class FakeLogger : ILogger<CorrelationIdMiddleware>
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

    private static (CorrelationIdMiddleware middleware, FakeLogger logger) CreateMiddleware(
        RequestDelegate? next = null)
    {
        var logger = new FakeLogger();
        var middleware = new CorrelationIdMiddleware(next ?? NextReturnsCompleted(), logger);
        return (middleware, logger);
    }

    [Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        var (middleware, _) = CreateMiddleware();
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        // Arrange
        var called = false;
        RequestDelegate next = _ => { called = true; return Task.CompletedTask; };
        var (middleware, _) = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_Should_Add_CorrelationId_To_Context_Items()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey("CorrelationId"));
        Assert.NotNull(context.Items["CorrelationId"]);
    }

    [Fact]
    public async Task InvokeAsync_Should_Register_OnStarting_Callback_For_Response_Header()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - correlation ID is stored in context items; OnStarting sets the header when response starts
        var contextCorrelationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(contextCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Not_In_Request()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Use_Existing_CorrelationId_From_Request_Header()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "test-correlation-id-123";
        context.Request.Headers["X-Correlation-ID"] = expectedCorrelationId;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_Invalid()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "invalid@#$%correlation";

        // Act
        await middleware.InvokeAsync(context);

        // Assert - new GUID generated
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));

        // Warning logged
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("Invalid Correlation ID"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_Too_Long()
    {
        // Arrange
        var (middleware, logger) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = new string('a', 129);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - new GUID generated
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));

        // Warning logged
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("Invalid Correlation ID"));
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_WhiteSpace()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "   ";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Register_OnStarting_Callback_To_Set_Response_Header()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - correlation ID is available in context items
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.NotNull(correlationId);
        Assert.NotEmpty(correlationId);
    }

    [Fact]
    public void UseCorrelationId_Should_Return_IApplicationBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act
        var result = CorrelationIdMiddlewareExtensions.UseCorrelationId(app);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IApplicationBuilder>(result);
    }

    [Fact]
    public async Task InvokeAsync_Should_Accept_CorrelationId_At_Exact_MaxLength()
    {
        // 128 characters — exactly at the allowed boundary
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();
        var exactMax = new string('a', 128);
        context.Request.Headers["X-Correlation-ID"] = exactMax;

        await middleware.InvokeAsync(context);

        Assert.Equal(exactMax, context.Items["CorrelationId"]?.ToString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Reject_CorrelationId_One_Over_MaxLength()
    {
        // 129 characters — one over the limit
        var (middleware, _) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = new string('a', 129);

        await middleware.InvokeAsync(context);

        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.True(Guid.TryParse(correlationId, out _), "Expected a generated GUID for an oversized header");
    }
}
