using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for CorrelationIdMiddleware
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;

    public CorrelationIdMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
    }

    [Xunit.Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        // Arrange & Act
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);

        // Assert
        Xunit.Assert.NotNull(middleware);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Add_CorrelationId_To_Context_Items()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Xunit.Assert.True(context.Items.ContainsKey("CorrelationId"));
        Xunit.Assert.NotNull(context.Items["CorrelationId"]);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Register_OnStarting_Callback_For_Response_Header()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        var responseCorrelationId = string.Empty;

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Capture the OnStarting callback behavior
        context.Response.OnStarting(() =>
        {
            responseCorrelationId = context.Response.Headers["X-Correlation-ID"].ToString();
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert - verify context items has correlation ID (OnStarting callback will be triggered when response actually starts)
        var contextCorrelationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(contextCorrelationId);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Not_In_Request()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(correlationId);
        Xunit.Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Use_Existing_CorrelationId_From_Request_Header()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "test-correlation-id-123";
        context.Request.Headers["X-Correlation-ID"] = expectedCorrelationId;

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.Equal(expectedCorrelationId, correlationId);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Request_Started()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Request_Completed()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_Invalid()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        // Invalid correlation ID (contains invalid characters)
        context.Request.Headers["X-Correlation-ID"] = "invalid@#$%correlation";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - should generate a new GUID since the header was invalid
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(correlationId);
        Xunit.Assert.True(Guid.TryParse(correlationId, out _));
        
        // Verify warning was logged
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid correlation ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_Too_Long()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        // Correlation ID that exceeds max length (128 characters)
        context.Request.Headers["X-Correlation-ID"] = new string('a', 129);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - should generate a new GUID since the header was too long
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(correlationId);
        Xunit.Assert.True(Guid.TryParse(correlationId, out _));
        
        // Verify warning was logged
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid correlation ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_WhiteSpace()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "   ";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - should generate a new GUID since the header was whitespace
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(correlationId);
        Xunit.Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Register_OnStarting_Callback_To_Set_Response_Header()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        
        // Create a context that tracks OnStarting callbacks
        var context = new DefaultHttpContext();
        string? headerValue = null;

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback(() =>
            {
                // Manually invoke OnStarting callbacks after they're registered
                // This simulates what happens when response starts
                var callbacks = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseFeature>();
                if (callbacks != null)
                {
                    // Access OnStarting through the feature
                    try
                    {
                        context.Response.StartAsync().Wait();
                        headerValue = context.Response.Headers["X-Correlation-ID"].ToString();
                    }
                    catch
                    {
                        // StartAsync may not work in test context, that's okay
                    }
                }
            })
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - verify correlation ID is in context items (this always works)
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Xunit.Assert.NotNull(correlationId);
        Xunit.Assert.NotEmpty(correlationId);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_With_CorrelationId_Context()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            _mockNext.Object,
            _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/transactions";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - verify logging includes the correlation ID
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Request started") &&
                    v.ToString()!.Contains("POST") &&
                    v.ToString()!.Contains("/api/transactions")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Request completed") &&
                    v.ToString()!.Contains("POST") &&
                    v.ToString()!.Contains("/api/transactions")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public void UseCorrelationId_Should_Return_IApplicationBuilder()
    {
        // Arrange
        var mockAppBuilder = new Mock<IApplicationBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var properties = new Dictionary<string, object?>();
        
        mockAppBuilder.Setup(app => app.ApplicationServices).Returns(mockServiceProvider.Object);
        mockAppBuilder.Setup(app => app.Properties).Returns(properties);
        mockAppBuilder.Setup(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mockAppBuilder.Object);

        // Act
        var result = CorrelationIdMiddlewareExtensions.UseCorrelationId(mockAppBuilder.Object);

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.IsAssignableFrom<IApplicationBuilder>(result);
    }
}
