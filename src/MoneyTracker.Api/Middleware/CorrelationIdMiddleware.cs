using System.Text.RegularExpressions;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Middleware for handling request correlation IDs across microservices.
/// Enriches the logging scope with the correlation ID so all downstream logs
/// automatically carry it without explicit passing.
/// </summary>
public partial class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdLogProperty = "CorrelationId";
    private const int MaxCorrelationIdLength = 128;

    [GeneratedRegex(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled)]
    private static partial Regex SafeCorrelationIdPattern();

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Store in context items for use by other components
        context.Items[CorrelationIdLogProperty] = correlationId;

        // Propagate to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Enrich ALL logs within this request with the correlation ID
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdLogProperty] = correlationId
        }))
        {
            await _next(context);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdHeader))
        {
            var correlationId = correlationIdHeader.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId)
                && correlationId.Length <= MaxCorrelationIdLength
                && SafeCorrelationIdPattern().IsMatch(correlationId))
            {
                LogCorrelationIdFromHeader(_logger, correlationId);
                return correlationId;
            }

            LogInvalidCorrelationId(_logger);
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        LogGeneratedCorrelationId(_logger, newCorrelationId);
        return newCorrelationId;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Correlation ID received from header: {CorrelationId}")]
    private static partial void LogCorrelationIdFromHeader(ILogger logger, string correlationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid Correlation ID received in header; generating a new one.")]
    private static partial void LogInvalidCorrelationId(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Generated new Correlation ID: {CorrelationId}")]
    private static partial void LogGeneratedCorrelationId(ILogger logger, string correlationId);
}

/// <summary>
/// Extension method to register correlation ID middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
