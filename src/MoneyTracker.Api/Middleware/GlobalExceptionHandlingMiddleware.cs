using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Models;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Global exception handling middleware. Catches all unhandled exceptions and
/// maps them to appropriate HTTP problem responses.
/// </summary>
public partial class GlobalExceptionHandlingMiddleware
{
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        LogUnhandledException(_logger, exception);

        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case FluentValidation.ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Code = "VALIDATION_ERROR";
                response.Message = "One or more validation errors occurred";
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Errors = validationEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());
                break;

            case UnauthorizedAccessException unauthorizedEx:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                response.Code = "FORBIDDEN";
                response.Message = unauthorizedEx.Message;
                response.StatusCode = StatusCodes.Status403Forbidden;
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Code = "CONFLICT";
                response.Message = conflictEx.Message;
                response.StatusCode = StatusCodes.Status409Conflict;
                break;

            case BadRequestException badRequestEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Code = "BAD_REQUEST";
                response.Message = badRequestEx.Message;
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case ArgumentException argumentEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Code = "BAD_REQUEST";
                response.Message = argumentEx.Message;
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Code = "INVALID_OPERATION";
                response.Message = "The operation is invalid";
                response.StatusCode = StatusCodes.Status400BadRequest;
                if (_environment.IsDevelopment())
                    response.Details = invalidOpEx.Message;
                break;

            case EntityNotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Code = "NOT_FOUND";
                response.Message = "The requested resource was not found";
                response.StatusCode = StatusCodes.Status404NotFound;
                if (_environment.IsDevelopment())
                    response.Details = notFoundEx.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Code = "INTERNAL_SERVER_ERROR";
                response.Message = "An internal server error occurred";
                response.StatusCode = StatusCodes.Status500InternalServerError;
                if (_environment.IsDevelopment())
                    response.Details = exception.ToString();
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception occurred.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
