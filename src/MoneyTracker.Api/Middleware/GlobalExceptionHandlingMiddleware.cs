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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            LogRequestCanceled(_logger);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
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

    private static Task WriteProblemResponseAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/problem+json";
        var payload = JsonSerializer.Serialize(problemDetails, problemDetails.GetType(), ProblemDetailsJsonOptions);
        return context.Response.WriteAsync(payload, context.RequestAborted);
    }

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
        .WithCommonExtensions(context, ErrorCodes.ValidationError);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, code, title, detail) = exception switch
        {
            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, ErrorMessageResources.Forbidden, GetDevelopmentDetail(unauthorizedEx.Message)),
            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ErrorMessageResources.Conflict, GetDevelopmentDetail(conflictEx.Message)),
            BadRequestException badRequestEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetDevelopmentDetail(badRequestEx.Message)),
            ArgumentException argumentEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessageResources.BadRequest, GetDevelopmentDetail(argumentEx.Message)),
            InvalidOperationException invalidOpEx =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidOperation, ErrorMessageResources.InvalidOperation, GetDevelopmentDetail(invalidOpEx.Message)),
            EntityNotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, ErrorCodes.NotFound, ErrorMessageResources.NotFound, GetDevelopmentDetail(notFoundEx.Message)),
            TimeoutException timeoutEx =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetDevelopmentDetail(timeoutEx.Message)),
            DbException dbException =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.TransientFailure, ErrorMessageResources.TransientFailure, GetDevelopmentDetail(dbException.Message)),
            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, ErrorMessageResources.InternalServerError, GetDevelopmentDetail(exception.ToString()))
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        }
        .WithCommonExtensions(context, code);
    }

    private string? GetDevelopmentDetail(string detail)
        => _environment.IsDevelopment() ? detail : null;

    [LoggerMessage(Level = LogLevel.Information, Message = "Request was canceled by the client.")]
    private static partial void LogRequestCanceled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception occurred.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The response has already started; the exception response cannot be written.")]
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
}



