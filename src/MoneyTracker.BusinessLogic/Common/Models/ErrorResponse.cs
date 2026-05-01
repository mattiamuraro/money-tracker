namespace MoneyTracker.BusinessLogic.Common.Models;

/// <summary>
/// Standard error response for all API errors
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error code (e.g., "INVALID_AMOUNT")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Technical details (only in development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Request trace ID for logging
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Field-level validation errors
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp when error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
