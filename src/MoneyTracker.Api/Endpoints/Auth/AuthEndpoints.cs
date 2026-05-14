using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    private const long AuthRequestBodySizeLimitBytes = 4 * 1024;

    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
                    .WithTags("Auth");

        group.MapPost("/login", static async (HttpContext httpContext, [FromServices] IHandler<LoginCommand, LoginAuthTokenDto> handler, LoginRequest request, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.ContentLength is > AuthRequestBodySizeLimitBytes)
                    return Results.Problem("Request payload is too large.", statusCode: StatusCodes.Status413PayloadTooLarge);

                var command = new LoginCommand
                {
                    Username = request.Username,
                    Password = request.Password
                };

                var result = await handler.Handle(command, cancellationToken);
                var response = new AuthTokenResponse
                {
                    Token = result.Token
                };

                return Results.Ok(response);
            })
            .WithName("Login")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", static async (HttpContext httpContext, [FromServices] IHandler<RegisterCommand, RegisterAuthTokenDto> handler, RegisterRequest request, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.ContentLength is > AuthRequestBodySizeLimitBytes)
                    return Results.Problem("Request payload is too large.", statusCode: StatusCodes.Status413PayloadTooLarge);

                var command = new RegisterCommand
                {
                    Username = request.Username,
                    Password = request.Password
                };

                var result = await handler.Handle(command, cancellationToken);
                var response = new AuthTokenResponse
                {
                    Token = result.Token
                };

                return Results.Ok(response);
            })
            .WithName("Register")
            .RequireRateLimiting("auth-register")
            .AllowAnonymous()
            .Accepts<RegisterRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/config", ([FromServices] IOptions<AuthOptions> authOptions) =>
                Results.Ok(new AuthConfigResponse { AllowRegistration = authOptions.Value.AllowRegistration }))
            .WithName("GetAuthConfig")
            .AllowAnonymous()
            .Produces<AuthConfigResponse>(StatusCodes.Status200OK);

        return app;
    }
}
