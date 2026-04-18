using MoneyTracker.Api.Auth;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Services;

namespace MoneyTracker.Api.Endpoints;

public static class AuthEndpoints
{
    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
                    .WithTags("Auth");

        group.MapPost("/login", async (AuthService authService, LoginRequest request, CancellationToken cancellationToken) =>
            await authService.LoginAsync(request.Username, request.Password, cancellationToken))
        .WithName("Login")
        .AllowAnonymous()
        .Accepts<LoginRequest>("application/json")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", async (AuthService authService, RegisterRequest request, CancellationToken cancellationToken) =>
            await authService.RegisterAsync(request.Username, request.Password, cancellationToken))
        .WithName("Register")
        .AllowAnonymous()
        .Accepts<RegisterRequest>("application/json")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapGet("/config", (AuthService authService) =>
            authService.GetAuthConfig())
        .WithName("GetAuthConfig")
        .AllowAnonymous()
        .Produces<object>(StatusCodes.Status200OK);

        return app;
    }
}
