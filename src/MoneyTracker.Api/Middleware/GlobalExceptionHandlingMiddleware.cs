using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
            Detail = GetDevelopmentDetail(exception.Message),
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

        LogUnhandledException(_logger, exception);

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
                (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, ErrorMessageResources.Unauthorized, GetDevelopmentDetail(unauthorizedEx.Message), ErrorCodes.Unauthorized, (string?)null, (string?)null),
            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ErrorMessageResources.Conflict, GetDevelopmentDetail(conflictEx.Message), conflictEx.ErrorCode ?? ErrorCodes.Conflict, conflictEx.EntityName, conflictEx.EntityId),
            BadRequestException badRequestEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetDevelopmentDetail(badRequestEx.Message), badRequestEx.ErrorCode ?? ErrorCodes.BadRequest, badRequestEx.EntityName, badRequestEx.EntityId),
            ArgumentException argumentEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetDevelopmentDetail(argumentEx.Message), ErrorCodes.BadRequest, (string?)null, (string?)null),
            InvalidOperationException invalidOpEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidOperation, ErrorMessageResources.InvalidOperation, GetDevelopmentDetail(invalidOpEx.Message), ErrorCodes.InvalidOperation, (string?)null, (string?)null),
            EntityNotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, ErrorCodes.NotFound, ErrorMessageResources.NotFound, GetDevelopmentDetail(notFoundEx.Message), notFoundEx.ErrorCode ?? ErrorCodes.NotFound, notFoundEx.EntityName, notFoundEx.EntityId),
            TimeoutException timeoutEx =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetDevelopmentDetail(timeoutEx.Message), ErrorCodes.TransientFailure, (string?)null, (string?)null),
            DbException dbException =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetDevelopmentDetail(dbException.Message), ErrorCodes.TransientFailure, (string?)null, (string?)null),
            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, ErrorMessageResources.InternalServerError, GetDevelopmentDetail(exception.ToString()), ErrorCodes.InternalServerError, (string?)null, (string?)null)
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

    private string? GetDevelopmentDetail(string detail)
    {
        if (!_environment.IsDevelopment())
        {
            return null;
        }

        return SanitizeDetail(detail);
    }

    private static string? SanitizeDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return detail;
        }

        var redactedDetail = detail;

        if (ContainsSensitiveKey(redactedDetail, "password")
            || ContainsSensitiveKey(redactedDetail, "pwd")
            || ContainsSensitiveKey(redactedDetail, "secret")
            || ContainsSensitiveKey(redactedDetail, "token")
            || ContainsSensitiveKey(redactedDetail, "apikey")
            || ContainsSensitiveKey(redactedDetail, "api-key")
            || ContainsSensitiveKey(redactedDetail, "connectionstring")
            || ContainsSensitiveKey(redactedDetail, "connection string"))
        {
            redactedDetail = "Sensitive details were redacted.";
        }

        return redactedDetail;
    }

    private static bool ContainsSensitiveKey(string value, string key)
        => value.Contains(key, StringComparison.OrdinalIgnoreCase);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Request was canceled by the client.")]
    private static partial void LogRequestCanceled(ILogger logger);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Error, Message = "An unhandled exception occurred.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);

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



