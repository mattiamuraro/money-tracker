using MoneyTracker.Api.Auth;
using MoneyTracker.Api.Contracts;

namespace MoneyTracker.Api.Endpoints;

public static class AuthEndpoints
{
    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
                    .WithTags("Auth");

        group.MapPost("/login", async (JwtTokenService tokenService, LoginRequest request, CancellationToken cancellationToken) =>
        {
            var token = await tokenService.LoginAsync(request.Username, request.Password, cancellationToken);
            if (token is null)
                return Results.Unauthorized();

            return Results.Ok(new { token });
        })
        .WithName("Login")
        .AllowAnonymous()
        .Accepts<LoginRequest>("application/json")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", async (
            JwtTokenService tokenService,
            RegisterRequest request,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            if (!bool.TryParse(configuration["Auth:AllowRegistration"], out var allowReg) || !allowReg)
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new ErrorResponse { Message = "Username and password are required.", StatusCode = 400 });

            var token = await tokenService.RegisterAsync(request.Username, request.Password, cancellationToken);
            if (token is null)
                return Results.Conflict(new ErrorResponse { Message = "Username is already taken.", StatusCode = 409 });

            return Results.Ok(new { token });
        })
        .WithName("Register")
        .AllowAnonymous()
        .Accepts<RegisterRequest>("application/json")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapGet("/config", (IConfiguration configuration) =>
        {
            var allowRegistration = bool.TryParse(configuration["Auth:AllowRegistration"], out var val) && val;
            return Results.Ok(new { allowRegistration });
        })
        .WithName("GetAuthConfig")
        .AllowAnonymous()
        .Produces<object>(StatusCodes.Status200OK);

        return app;
    }
}
