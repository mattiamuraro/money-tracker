using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandlingMiddleware
/// </summary>
public class GlobalExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public GlobalExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
    }

    [Xunit.Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        // Arrange & Act
        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        // Assert
        Xunit.Assert.NotNull(middleware);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate_When_No_Exception()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
        var context = new DefaultHttpContext();

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Handle_Exception_When_Next_Throws()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test exception");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Set_Response_ContentType_When_Exception_Occurs()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test exception");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Xunit.Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Not_Throw_When_Next_Throws_Exception()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test exception");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act & Assert - should not throw
        await middleware.InvokeAsync(context);
    }
}
