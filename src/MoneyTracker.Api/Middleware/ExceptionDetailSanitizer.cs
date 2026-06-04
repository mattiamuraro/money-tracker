using System.Text.RegularExpressions;

namespace MoneyTracker.Api.Middleware;

public partial class ExceptionDetailSanitizer : IExceptionDetailSanitizer
{
    private static readonly Regex ConnectionStringPasswordPattern = ConnectionStringPasswordRegex();
    private static readonly Regex JsonSecretPattern = JsonSecretRegex();
    private static readonly Regex BearerTokenPattern = BearerTokenRegex();
    private static readonly Regex ApiKeyPattern = ApiKeyRegex();
    private static readonly Regex KeyValuePattern = KeyValueRegex();
    private static readonly Regex HighEntropyTokenPattern = HighEntropyTokenRegex();

    public string? Sanitize(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return detail;
        }

        var sanitized = detail;
        sanitized = ConnectionStringPasswordPattern.Replace(sanitized, "$1[REDACTED]");
        sanitized = JsonSecretPattern.Replace(sanitized, "\"$1\":\"[REDACTED]\"");
        sanitized = BearerTokenPattern.Replace(sanitized, "$1 [REDACTED]");
        sanitized = ApiKeyPattern.Replace(sanitized, "$1[REDACTED]");
        sanitized = KeyValuePattern.Replace(sanitized, "$1=[REDACTED]");
        sanitized = HighEntropyTokenPattern.Replace(sanitized, "[REDACTED]");

        return sanitized;
    }

    [GeneratedRegex("(?i)(Password\\s*=\\s*)[^;\\s]+")]
    private static partial Regex ConnectionStringPasswordRegex();

    [GeneratedRegex("(?i)\\\"(password|pwd|secret|token|apiKey|api-key|connectionString)\\\"\\s*:\\s*\\\"[^\\\"]*\\\"")]
    private static partial Regex JsonSecretRegex();

    [GeneratedRegex("(?i)(Bearer)\\s+[A-Za-z0-9-_\\.]+")]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex("(?i)(x-api-key\\s*[:=]\\s*)[^,;\\s]+")]
    private static partial Regex ApiKeyRegex();

    [GeneratedRegex("(?i)(password|pwd|secret|token|apikey|api-key|connectionstring)\\s*=\\s*[^,;\\s]+")]
    private static partial Regex KeyValueRegex();

    [GeneratedRegex("[A-Za-z0-9+/=_-]{32,}")]
    private static partial Regex HighEntropyTokenRegex();
}
