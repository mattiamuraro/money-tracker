using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Options;
using MoneyTracker.Api.Resources;
using MoneyTracker.BusinessLogic.Common.Exceptions;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Global exception handling middleware. Catches all unhandled exceptions and
/// maps them to appropriate HTTP problem responses.
/// </summary>
public partial class GlobalExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions ProblemDetailsJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ExceptionDetailOptions _exceptionDetailOptions;
    private readonly IExceptionDetailSanitizer _exceptionDetailSanitizer;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment,
        IOptions<ExceptionDetailOptions> exceptionDetailOptions,
        IExceptionDetailSanitizer exceptionDetailSanitizer)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _exceptionDetailOptions = exceptionDetailOptions.Value;
        _exceptionDetailSanitizer = exceptionDetailSanitizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException cancellationEx) when (context.RequestAborted.IsCancellationRequested)
        {
            LogRequestCanceled(_logger);
            await HandleCanceledRequestAsync(context, cancellationEx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleCanceledRequestAsync(HttpContext context, OperationCanceledException exception)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["Path"] = context.Request.Path.Value ?? string.Empty
        });

        if (context.Response.HasStarted)
        {
            LogResponseAlreadyStarted(_logger);
            return Task.CompletedTask;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status408RequestTimeout,
            Title = ErrorMessageResources.RequestCanceled,
            Detail = GetClientDetail(exception.Message),
            Type = $"https://httpstatuses.com/{StatusCodes.Status408RequestTimeout}",
            Instance = context.Request.Path
        }
        .WithCommonExtensions(context, ErrorCodes.RequestCanceled)
        .WithExceptionMetadata(ErrorCodes.RequestCanceled, null, null);

        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
        return WriteProblemResponseAsync(context, problemDetails, CancellationToken.None);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["Path"] = context.Request.Path.Value ?? string.Empty
        });

        if (IsHandledException(exception))
        {
            LogHandledException(_logger, exception.GetType().Name, exception.Message);
        }
        else
        {
            LogUnhandledException(_logger, exception);
        }

        if (context.Response.HasStarted)
        {
            LogResponseAlreadyStarted(_logger);
            return Task.CompletedTask;
        }

        if (exception is FluentValidation.ValidationException validationEx)
        {
            var validationProblem = CreateValidationProblemDetails(context, validationEx);
            context.Response.StatusCode = validationProblem.Status ?? StatusCodes.Status400BadRequest;
            return WriteProblemResponseAsync(context, validationProblem);
        }

        var problemDetails = CreateProblemDetails(context, exception);
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        return WriteProblemResponseAsync(context, problemDetails);
    }

    private static bool IsHandledException(Exception exception)
        => exception is FluentValidation.ValidationException
            or UnauthorizedAccessException
            or ConflictException
            or BadRequestException
            or ArgumentException
            or EntityNotFoundException;

    private static Task WriteProblemResponseAsync(HttpContext context, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/problem+json";
        var payload = JsonSerializer.Serialize(problemDetails, problemDetails.GetType(), ProblemDetailsJsonOptions);
        return context.Response.WriteAsync(payload, cancellationToken);
    }

    private static Task WriteProblemResponseAsync(HttpContext context, ProblemDetails problemDetails)
        => WriteProblemResponseAsync(context, problemDetails, context.RequestAborted);

    private ValidationProblemDetails CreateValidationProblemDetails(HttpContext context, FluentValidation.ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ErrorMessageResources.ValidationError,
            Type = "https://httpstatuses.com/400",
            Instance = context.Request.Path
        }
        .WithCommonExtensions(context, ErrorCodes.ValidationError)
        .WithExceptionMetadata(ErrorCodes.ValidationError, null, null);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, code, title, detail, subCode, entityName, entityId) = exception switch
        {
            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, ErrorMessageResources.Unauthorized, GetClientDetail(unauthorizedEx.Message), ErrorCodes.Unauthorized, (string?)null, (string?)null),
            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ErrorMessageResources.Conflict, GetClientDetail(conflictEx.Message), conflictEx.ErrorCode ?? ErrorCodes.Conflict, conflictEx.EntityName, conflictEx.EntityId),
            BadRequestException badRequestEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetClientDetail(badRequestEx.Message), badRequestEx.ErrorCode ?? ErrorCodes.BadRequest, badRequestEx.EntityName, badRequestEx.EntityId),
            ArgumentException argumentEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetClientDetail(argumentEx.Message), ErrorCodes.BadRequest, (string?)null, (string?)null),
            EntityNotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, ErrorCodes.NotFound, ErrorMessageResources.NotFound, GetClientDetail(notFoundEx.Message), notFoundEx.ErrorCode ?? ErrorCodes.NotFound, notFoundEx.EntityName, notFoundEx.EntityId),
            TimeoutException timeoutEx =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetClientDetail(timeoutEx.Message), ErrorCodes.TransientFailure, (string?)null, (string?)null),
            DbException dbException => ClassifyDbException(dbException),
            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, ErrorMessageResources.InternalServerError, GetClientDetail(exception.Message), ErrorCodes.InternalServerError, (string?)null, (string?)null)
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        }
        .WithCommonExtensions(context, code)
        .WithExceptionMetadata(subCode, entityName, entityId);
    }

    private (int StatusCode, string Code, string Title, string? Detail, string? SubCode, string? EntityName, string? EntityId) ClassifyDbException(DbException dbException)
    {
        var classification = GetDbExceptionClassification(dbException);

        _logger.LogWarning(
            new EventId(1105, "DbExceptionClassified"),
            dbException,
            "Database exception classified as {Classification} with status code {StatusCode}.",
            classification,
            classification switch
            {
                DbExceptionClassification.Transient => StatusCodes.Status503ServiceUnavailable,
                DbExceptionClassification.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            });

        return classification switch
        {
            DbExceptionClassification.Transient =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetClientDetail(dbException.Message), ErrorCodes.TransientFailure, (string?)null, (string?)null),
            DbExceptionClassification.Conflict =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ErrorMessageResources.Conflict, GetClientDetail(dbException.Message), ErrorCodes.Conflict, (string?)null, (string?)null),
            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, ErrorMessageResources.InternalServerError, GetClientDetail(dbException.Message), ErrorCodes.InternalServerError, (string?)null, (string?)null)
        };
    }

    private static DbExceptionClassification GetDbExceptionClassification(DbException dbException)
    {
        var message = dbException.Message;

        if (ContainsAny(message,
                "timeout",
                "deadlock",
                "transport-level",
                "connection is broken",
                "network-related",
                "temporarily unavailable",
                "transient"))
        {
            return DbExceptionClassification.Transient;
        }

        if (ContainsAny(message,
                "unique constraint",
                "duplicate key",
                "foreign key",
                "check constraint",
                "violates",
                "primary key"))
        {
            return DbExceptionClassification.Conflict;
        }

        return DbExceptionClassification.Unknown;
    }

    private static bool ContainsAny(string value, params string[] markers)
        => markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private enum DbExceptionClassification
    {
        Unknown,
        Transient,
        Conflict
    }

    private string? GetClientDetail(string? detail)
    {
        if (!_environment.IsDevelopment() || !_exceptionDetailOptions.IncludeExceptionDetails)
        {
            return null;
        }

        return _exceptionDetailSanitizer.Sanitize(detail);
    }

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Request was canceled by the client.")]
    private static partial void LogRequestCanceled(ILogger logger);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Error, Message = "An unhandled exception occurred.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Warning, Message = "A handled exception occurred: {ExceptionType}. Message: {Message}")]
    private static partial void LogHandledException(ILogger logger, string exceptionType, string message);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Warning, Message = "The response has already started; the exception response cannot be written.")]
    private static partial void LogResponseAlreadyStarted(ILogger logger);
}

internal static class ProblemDetailsExtensions
{
    private const string CorrelationIdItemKey = "CorrelationId";

    public static T WithCommonExtensions<T>(this T problemDetails, HttpContext context, string code)
        where T : ProblemDetails
    {
        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        if (context.Items.TryGetValue(CorrelationIdItemKey, out var correlationId)
            && correlationId is string correlationIdValue
            && !string.IsNullOrWhiteSpace(correlationIdValue))
        {
            problemDetails.Extensions["correlationId"] = correlationIdValue;
        }

        return problemDetails;
    }

    public static T WithExceptionMetadata<T>(this T problemDetails, string? subCode, string? entityName, string? entityId)
        where T : ProblemDetails
    {
        if (!string.IsNullOrWhiteSpace(subCode))
        {
            problemDetails.Extensions["subCode"] = subCode;
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            problemDetails.Extensions["entityName"] = entityName;
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            problemDetails.Extensions["entityId"] = entityId;
        }

        return problemDetails;
    }
}



