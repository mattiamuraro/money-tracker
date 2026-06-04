using System.Diagnostics;

namespace MoneyTracker.Api.Middleware;

public class RequestObservabilityMiddleware(RequestDelegate next)
{
    private const string CorrelationIdItemKey = "CorrelationId";
    private const string TraceIdScopeKey = "TraceId";
    private const string CorrelationIdScopeKey = "CorrelationId";
    private const string EndpointScopeKey = "Endpoint";
    private const string HttpMethodScopeKey = "HttpMethod";
    private const string PathScopeKey = "Path";

    public async Task InvokeAsync(HttpContext context, ILogger<RequestObservabilityMiddleware> logger)
    {
        var endpoint = context.GetEndpoint()?.DisplayName ?? "unknown";
        var correlationId = context.Items.TryGetValue(CorrelationIdItemKey, out var correlationIdValue)
            ? correlationIdValue as string
            : null;

        var activity = Activity.Current;
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("endpoint.name", endpoint);
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.route", context.Request.Path.Value);

        var startedAt = Stopwatch.GetTimestamp();
        using (logger.BeginScope(new Dictionary<string, object?>
        {
            [TraceIdScopeKey] = context.TraceIdentifier,
            [CorrelationIdScopeKey] = correlationId,
            [EndpointScopeKey] = endpoint,
            [HttpMethodScopeKey] = context.Request.Method,
            [PathScopeKey] = context.Request.Path.Value
        }))
        {
            await next(context);

            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            var statusCode = context.Response.StatusCode;
            activity?.SetTag("http.status_code", statusCode);
            activity?.SetTag("http.response.elapsed_ms", elapsed.TotalMilliseconds);

            logger.LogInformation(
                new EventId(1301, "RequestCompleted"),
                "Request completed with status code {StatusCode} in {ElapsedMs} ms.",
                statusCode,
                elapsed.TotalMilliseconds);
        }
    }
}

public static class RequestObservabilityMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestObservability(this IApplicationBuilder app)
        => app.UseMiddleware<RequestObservabilityMiddleware>();
}
