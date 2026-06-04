namespace MoneyTracker.Api.Middleware;

public interface IExceptionDetailSanitizer
{
    string? Sanitize(string? detail);
}
