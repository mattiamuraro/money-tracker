using System.Diagnostics;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private const int MaxLoggableBodySizeBytes = 65536; // 64 KB

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request
        var requestBody = await ReadRequestBody(context.Request);
        _logger.LogInformation(
            "Request: {Method} {Path} {QueryString}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        if (!string.IsNullOrEmpty(requestBody) && context.Request.ContentType?.Contains("application/json") == true)
        {
            _logger.LogDebug("Request Body: {Body}", requestBody);
        }

        // Store original response body stream
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Read response body
            var response = await ReadResponseBody(responseBody);

            // Log response
            _logger.LogInformation(
                "Response: {StatusCode} {Method} {Path} completed in {ElapsedMs}ms",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            if (!string.IsNullOrEmpty(response) && context.Response.ContentType?.Contains("application/json") == true)
            {
                _logger.LogDebug("Response Body: {Body}", response);
            }

            // Copy response to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength.HasValue && request.ContentLength > MaxLoggableBodySizeBytes)
            return "[Body too large to log]";

        request.EnableBuffering();
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private async Task<string> ReadResponseBody(MemoryStream responseBody)
    {
        if (responseBody.Length > MaxLoggableBodySizeBytes)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            return "[Body too large to log]";
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        return body;
    }
}
