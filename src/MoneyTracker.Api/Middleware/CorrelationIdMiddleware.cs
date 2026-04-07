namespace MoneyTracker.ApiService.Middleware;

/// <summary>
/// Middleware for handling request correlation IDs across microservices
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdLogProperty = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get existing or generate new correlation ID
        var correlationId = GetOrCreateCorrelationId(context);

        // Add to context items for use throughout the request
        context.Items[CorrelationIdLogProperty] = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        _logger.LogInformation(
            "Request started: {Method} {Path} [CorrelationId: {CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await _next(context);

        _logger.LogInformation(
            "Request completed: {Method} {Path} -> {StatusCode} [CorrelationId: {CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            correlationId);
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get from request header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdHeader))
        {
            var correlationId = correlationIdHeader.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogDebug("Correlation ID from header: {CorrelationId}", correlationId);
                return correlationId;
            }
        }

        // Generate new correlation ID
        var newCorrelationId = Guid.NewGuid().ToString();
        _logger.LogDebug("Generated new Correlation ID: {CorrelationId}", newCorrelationId);
        return newCorrelationId;
    }
}

/// <summary>
/// Extension method to add correlation ID middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
