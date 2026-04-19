using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionHandlingMiddleware
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
        _logger.LogError(exception, "An unhandled exception occurred");

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

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Code = "INVALID_OPERATION";
                response.Message = "The operation is invalid";
                response.StatusCode = StatusCodes.Status400BadRequest;
                if (_environment.IsDevelopment())
                    response.Details = invalidOpEx.Message;
                break;

            case KeyNotFoundException notFoundEx:
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
}
