using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker.Api.Middleware;

public class JsonContentTypeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresJsonContentType(context.Request))
        {
            await next(context);
            return;
        }

        if (IsJsonContentType(context.Request.ContentType))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status415UnsupportedMediaType,
            Title = "Unsupported Media Type",
            Detail = "This endpoint accepts only JSON payloads.",
            Type = "https://httpstatuses.com/415",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }

    private static bool RequiresJsonContentType(HttpRequest request)
    {
        if (!request.Path.StartsWithSegments("/api/v1", StringComparison.OrdinalIgnoreCase))
            return false;

        return HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method);
    }

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("+json", StringComparison.OrdinalIgnoreCase);
    }
}

public static class JsonContentTypeMiddlewareExtensions
{
    /// <summary>
    /// Enforces JSON content type for API write operations.
    /// </summary>
    public static IApplicationBuilder UseJsonContentTypeEnforcement(this IApplicationBuilder app)
        => app.UseMiddleware<JsonContentTypeMiddleware>();
}
