using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Auth, "Auth");

        group.MapPost("/login", static async ([FromServices] IHandler<LoginCommand, LoginAuthTokenDto> handler, LoginRequest request, CancellationToken cancellationToken) =>
            {
                var command = new LoginCommand
                {
                    Username = request.Username,
                    Password = request.Password
                };

                var result = await handler.Handle(command, cancellationToken);
                var response = new AuthTokenResponse
                {
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                };

                return Results.Ok(response);
            })
            .RequireAuthRequestSizeLimit()
            .AddNoStoreResponseHeaders()
            .WithName("Login")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", static async ([FromServices] IHandler<RegisterCommand, RegisterAuthTokenDto> handler, RegisterRequest request, CancellationToken cancellationToken) =>
            {
                var command = new RegisterCommand
                {
                    Username = request.Username,
                    Password = request.Password
                };

                var result = await handler.Handle(command, cancellationToken);
                var response = new AuthTokenResponse
                {
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                };

                return Results.Ok(response);
            })
            .RequireAuthRequestSizeLimit()
            .AddNoStoreResponseHeaders()
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

        group.MapPost("/refresh", static async ([FromServices] IHandler<RefreshTokenCommand, RefreshAuthTokenDto> handler, RefreshTokenRequest request, CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                }, cancellationToken);

                return Results.Ok(new AuthTokenResponse
                {
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                });
            })
            .RequireAuthRequestSizeLimit()
            .AddNoStoreResponseHeaders()
            .WithName("Refresh")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/revoke", static async ([FromServices] IHandler<RevokeRefreshTokenCommand, bool> handler, RefreshTokenRequest request, CancellationToken cancellationToken) =>
            {
                await handler.Handle(new RevokeRefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                }, cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthRequestSizeLimit()
            .AddNoStoreResponseHeaders()
            .WithName("Revoke")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/change-password", static async (HttpContext httpContext, [FromServices] IHandler<ChangePasswordCommand, bool> handler, ChangePasswordRequest request, CancellationToken cancellationToken) =>
            {
                await handler.Handle(new ChangePasswordCommand
                {
                    UserId = httpContext.GetCurrentUserId(),
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword
                }, cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthRequestSizeLimit()
            .AddNoStoreResponseHeaders()
            .WithName("ChangePassword")
            .RequireAuthorization()
            .Accepts<ChangePasswordRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/config", ([FromServices] IOptions<AuthOptions> authOptions) =>
                Results.Ok(new AuthConfigResponse { AllowRegistration = authOptions.Value.AllowRegistration }))
            .WithName("GetAuthConfig")
            .AllowAnonymous()
            .Produces<AuthConfigResponse>(StatusCodes.Status200OK);

        return app;
    }
}
