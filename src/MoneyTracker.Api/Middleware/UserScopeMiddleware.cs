using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MoneyTracker.Api.Middleware;

/// <summary>
/// Middleware that enriches the logging scope with the authenticated user's identity
/// (UserId and Username) so every log line within an authenticated request automatically
/// carries the user context. Must run after UseAuthentication().
/// </summary>
public partial class UserScopeMiddleware(RequestDelegate next)
{
    private const string UserIdProperty = "UserId";
    private const string UsernameProperty = "Username";

    public async Task InvokeAsync(HttpContext context, ILogger<UserScopeMiddleware> logger)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = user.FindFirstValue(ClaimTypes.Name);

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                [UserIdProperty] = HashForLogging(userId),
                [UsernameProperty] = HashForLogging(username)
            }))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }

    private static string? HashForLogging(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes.AsSpan(0, 8));
    }
}

public static class UserScopeMiddlewareExtensions
{
    /// <summary>
    /// Adds user identity enrichment to the logging scope.
    /// Must be called AFTER UseAuthentication() and UseAuthorization().
    /// </summary>
    public static IApplicationBuilder UseUserScope(this IApplicationBuilder app)
        => app.UseMiddleware<UserScopeMiddleware>();
}