using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MoneyTracker.Api.Middleware;
using System.Text;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for RequestResponseLoggingMiddleware
/// </summary>
public class RequestResponseLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestResponseLoggingMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;

    public RequestResponseLoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
    }

    [Xunit.Fact]
    public void Constructor_Should_Assign_Dependencies()
    {
        // Arrange & Act
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);

        // Assert
        Xunit.Assert.NotNull(middleware);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Request_Information()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", "?foo=bar");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request:") && v.ToString()!.Contains("GET") && v.ToString()!.Contains("/api/test")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Response_Information()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("POST", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx => ctx.Response.StatusCode = 200)
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response:") && v.ToString()!.Contains("200") && v.ToString()!.Contains("completed in")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Request_Body_When_Json_Content_Type()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var requestBody = "{\"test\":\"value\"}";
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, "application/json");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request Body:") && v.ToString()!.Contains(requestBody)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Not_Log_Request_Body_When_Not_Json_Content_Type()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var requestBody = "plain text body";
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, "text/plain");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Not_Log_Request_Body_When_Empty()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Response_Body_When_Json_Content_Type()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var responseBody = "{\"result\":\"success\"}";
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx =>
            {
                ctx.Response.ContentType = "application/json";
                var bytes = Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.Body.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response Body:") && v.ToString()!.Contains(responseBody)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Not_Log_Response_Body_When_Not_Json_Content_Type()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var responseBody = "plain text response";
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx =>
            {
                ctx.Response.ContentType = "text/plain";
                var bytes = Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.Body.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Not_Log_Response_Body_When_Empty()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx => ctx.Response.ContentType = "application/json")
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Copy_Response_To_Original_Stream()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var responseBody = "test response";
        var context = CreateHttpContext("GET", "/api/test", string.Empty);
        var originalStream = new MemoryStream();
        context.Response.Body = originalStream;

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx =>
            {
                var bytes = Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.Body.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        originalStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(originalStream);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Xunit.Assert.Equal(responseBody, content);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_Elapsed_Time()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(async () => await Task.Delay(50));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Handle_Null_Request_ContentType()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var requestBody = "{\"test\":\"value\"}";
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, null);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Handle_Null_Response_ContentType()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var responseBody = "{\"result\":\"success\"}";
        var context = CreateHttpContext("GET", "/api/test", string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx =>
            {
                ctx.Response.ContentType = null;
                var bytes = Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.Body.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response Body:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Log_QueryString_When_Present()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("GET", "/api/test", "?param1=value1&param2=value2");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("?param1=value1&param2=value2")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Xunit.Fact]
    public async Task InvokeAsync_Should_Handle_Json_ContentType_With_Charset()
    {
        // Arrange
        var middleware = new RequestResponseLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var requestBody = "{\"test\":\"value\"}";
        var context = CreateHttpContext("POST", "/api/test", string.Empty, requestBody, "application/json; charset=utf-8");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request Body:") && v.ToString()!.Contains(requestBody)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
}
