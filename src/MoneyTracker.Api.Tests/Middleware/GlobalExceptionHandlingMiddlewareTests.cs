using System.Data.Common;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MoneyTracker.Api.Middleware;
using MoneyTracker.Api.Options;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandlingMiddleware
/// </summary>
public class GlobalExceptionHandlingMiddlewareTests
{
    private sealed class TestDbException(string message) : DbException(message);

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
        IWebHostEnvironment? environment = null,
        bool includeExceptionDetails = true)
    {
        var logger = new FakeLogger();
        var env = environment ?? new FakeWebHostEnvironment();
        var options = Microsoft.Extensions.Options.Options.Create(new ExceptionDetailOptions { IncludeExceptionDetails = includeExceptionDetails });
        var sanitizer = new ExceptionDetailSanitizer();
        var middleware = new GlobalExceptionHandlingMiddleware(next ?? (_ => Task.CompletedTask), logger, env, options, sanitizer);
        return (middleware, logger);
    }

    // Creates a context whose Response.HasStarted is already true via a fake response feature.
    private static DefaultHttpContext CreateStartedResponseContext()
    {
        var context = new DefaultHttpContext();
        var responseFeature = new StartedHttpResponseFeature();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Response.Body = new MemoryStream();
        return context;
    }

    private sealed class StartedHttpResponseFeature : IHttpResponseFeature
    {
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted => true;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string? ReasonPhrase { get; set; }
        public int StatusCode { get; set; } = 200;

        public void OnCompleted(Func<object, Task> callback, object state) { }
        public void OnStarting(Func<object, Task> callback, object state) { }
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
    public async Task InvokeAsync_Should_Set_ProblemDetails_ContentType_When_Exception_Occurs()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("Test exception"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_InternalServerError_When_Exception_Occurs()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("Test exception"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
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

    [Theory]
    [InlineData(typeof(BadRequestException), StatusCodes.Status400BadRequest, "BAD_REQUEST")]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized, "UNAUTHORIZED")]
    [InlineData(typeof(EntityNotFoundException), StatusCodes.Status404NotFound, "NOT_FOUND")]
    [InlineData(typeof(ConflictException), StatusCodes.Status409Conflict, "CONFLICT")]
    public async Task InvokeAsync_Should_Map_Known_Exceptions_To_Expected_Status_And_Code(Type exceptionType, int expectedStatus, string expectedCode)
    {
        Exception exception = exceptionType == typeof(BadRequestException)
            ? new BadRequestException("bad")
            : exceptionType == typeof(UnauthorizedAccessException)
                ? new UnauthorizedAccessException("unauthorized")
                : exceptionType == typeof(EntityNotFoundException)
                    ? new EntityNotFoundException("missing")
                    : new ConflictException("conflict");

        var (middleware, _) = CreateMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CorrelationId"] = "corr-123";

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedCode, doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("corr-123", doc.RootElement.GetProperty("correlationId").GetString());
        Assert.True(doc.RootElement.TryGetProperty("traceId", out _));
        Assert.True(doc.RootElement.TryGetProperty("timestamp", out _));
        Assert.True(doc.RootElement.TryGetProperty("subCode", out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_ValidationProblemDetails_When_ValidationException_Thrown()
    {
        var validationException = new ValidationException([
            new ValidationFailure("Month", "Month is required")
        ]);

        var (middleware, _) = CreateMiddleware(_ => throw validationException);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("VALIDATION_ERROR", doc.RootElement.GetProperty("subCode").GetString());
        Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Month", out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_RequestTimeout_ProblemDetails_When_ValidationException_Thrown()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var (middleware, logger) = CreateMiddleware(_ => throw new OperationCanceledException(cts.Token));
        var context = new DefaultHttpContext();
        context.RequestAborted = cts.Token;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status408RequestTimeout, context.Response.StatusCode);
        Assert.Equal("REQUEST_CANCELED", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("REQUEST_CANCELED", doc.RootElement.GetProperty("subCode").GetString());
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task InvokeAsync_Should_Include_Metadata_Extensions_For_Domain_Exceptions()
    {
        var exception = new EntityNotFoundException(
            "payment not found",
            errorCode: "PAYMENT_NOT_FOUND",
            entityName: "Payment",
            entityId: "123");

        var (middleware, _) = CreateMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal("PAYMENT_NOT_FOUND", doc.RootElement.GetProperty("subCode").GetString());
        Assert.Equal("Payment", doc.RootElement.GetProperty("entityName").GetString());
        Assert.Equal("123", doc.RootElement.GetProperty("entityId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Redact_Sensitive_Development_Details()
    {
        var env = new FakeWebHostEnvironment { EnvironmentName = "Development" };
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("password=super-secret"), env);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal("password=[REDACTED]", doc.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Redact_Bearer_And_Api_Key_Details()
    {
        var env = new FakeWebHostEnvironment { EnvironmentName = "Development" };
        var rawMessage = "Authorization: Bearer abcdefghijklmnopqrstuvwxyz123456; x-api-key=topsecret";
        var (middleware, _) = CreateMiddleware(_ => throw new Exception(rawMessage), env);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        var detail = doc.RootElement.GetProperty("detail").GetString();
        Assert.NotNull(detail);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz123456", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("topsecret", detail, StringComparison.Ordinal);
        Assert.Contains("Bearer [REDACTED]", detail, StringComparison.Ordinal);
        Assert.Contains("x-api-key=[REDACTED]", detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_Should_Redact_Json_Secret_Details()
    {
        var env = new FakeWebHostEnvironment { EnvironmentName = "Development" };
        var rawMessage = "payload= {\"token\":\"abc123\",\"password\":\"super-secret\"}";
        var (middleware, _) = CreateMiddleware(_ => throw new Exception(rawMessage), env);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        var detail = doc.RootElement.GetProperty("detail").GetString();
        Assert.NotNull(detail);
        Assert.DoesNotContain("abc123", detail, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret", detail, StringComparison.Ordinal);
        Assert.Contains("\"token\":\"[REDACTED]\"", detail, StringComparison.Ordinal);
        Assert.Contains("\"password\":\"[REDACTED]\"", detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Include_Detail_In_Production()
    {
        var env = new FakeWebHostEnvironment { EnvironmentName = "Production" };
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("password=super-secret"), env);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.False(doc.RootElement.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Include_Detail_When_Option_Disabled()
    {
        var env = new FakeWebHostEnvironment { EnvironmentName = "Development" };
        var (middleware, _) = CreateMiddleware(_ => throw new Exception("password=super-secret"), env, includeExceptionDetails: false);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.False(doc.RootElement.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task InvokeAsync_Should_Map_InvalidOperationException_To_InternalServerError()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new InvalidOperationException("Unexpected state"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("INTERNAL_SERVER_ERROR", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("INTERNAL_SERVER_ERROR", doc.RootElement.GetProperty("subCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Map_Transient_DbException_To_ServiceUnavailable()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new TestDbException("A transient timeout occurred while connecting to database."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Equal("TRANSIENT_FAILURE", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("TRANSIENT_FAILURE", doc.RootElement.GetProperty("subCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Map_Constraint_DbException_To_Conflict()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new TestDbException("Violation of UNIQUE CONSTRAINT on table Payments."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("CONFLICT", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("CONFLICT", doc.RootElement.GetProperty("subCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Map_Unknown_DbException_To_InternalServerError()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new TestDbException("Unknown database engine failure."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("INTERNAL_SERVER_ERROR", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("INTERNAL_SERVER_ERROR", doc.RootElement.GetProperty("subCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Skip_Writing_And_Log_Warning_When_Response_Has_Started_And_Exception_Occurs()
    {
        // Arrange – context.Response.HasStarted is already true
        var (middleware, logger) = CreateMiddleware(_ => throw new Exception("late exception"));
        var context = CreateStartedResponseContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – middleware logs a warning about the already-started response
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("response has already started", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InvokeAsync_Should_Skip_Writing_And_Log_Warning_When_Response_Has_Started_And_Request_Canceled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var (middleware, logger) = CreateMiddleware(_ => throw new OperationCanceledException(cts.Token));
        var context = CreateStartedResponseContext();
        context.RequestAborted = cts.Token;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("response has already started", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_InternalServerError_When_OperationCanceledException_And_Request_Not_Aborted()
    {
        // Arrange – cancellation token is not the request-abort token, so it should be treated as unhandled
        var otherCts = new CancellationTokenSource();
        otherCts.Cancel();

        var (middleware, logger) = CreateMiddleware(_ => throw new OperationCanceledException(otherCts.Token));
        var context = new DefaultHttpContext();
        // RequestAborted is NOT canceled
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – must be a 500, not a 408
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task InvokeAsync_Should_Map_TimeoutException_To_ServiceUnavailable()
    {
        // Arrange
        var (middleware, _) = CreateMiddleware(_ => throw new TimeoutException("The operation timed out."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Equal("TRANSIENT_FAILURE", doc.RootElement.GetProperty("code").GetString());
        Assert.Equal("TRANSIENT_FAILURE", doc.RootElement.GetProperty("subCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Include_Metadata_Extensions_For_ConflictException_With_Custom_ErrorCode()
    {
        // Arrange
        var exception = new ConflictException(
            "Category already exists",
            errorCode: "CATEGORY_ALREADY_EXISTS",
            entityName: "Category",
            entityId: "42");

        var (middleware, _) = CreateMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("CATEGORY_ALREADY_EXISTS", doc.RootElement.GetProperty("subCode").GetString());
        Assert.Equal("Category", doc.RootElement.GetProperty("entityName").GetString());
        Assert.Equal("42", doc.RootElement.GetProperty("entityId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Include_Metadata_Extensions_For_BadRequestException_With_Custom_ErrorCode()
    {
        // Arrange
        var exception = new BadRequestException(
            "Invalid date range",
            errorCode: "INVALID_DATE_RANGE",
            entityName: "Budget",
            entityId: "99");

        var (middleware, _) = CreateMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("INVALID_DATE_RANGE", doc.RootElement.GetProperty("subCode").GetString());
        Assert.Equal("Budget", doc.RootElement.GetProperty("entityName").GetString());
        Assert.Equal("99", doc.RootElement.GetProperty("entityId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_ValidationProblemDetails_With_Multiple_Fields_Grouped_By_Property()
    {
        // Arrange – two failures on the same field and one on another
        var validationException = new ValidationException([
            new ValidationFailure("Amount", "Amount must be positive"),
            new ValidationFailure("Amount", "Amount must not exceed 1000000"),
            new ValidationFailure("Date", "Date is required")
        ]);

        var (middleware, _) = CreateMiddleware(_ => throw validationException);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", doc.RootElement.GetProperty("code").GetString());

        var errors = doc.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Amount", out var amountErrors));
        Assert.Equal(2, amountErrors.GetArrayLength());
        Assert.True(errors.TryGetProperty("Date", out _));
    }
}

