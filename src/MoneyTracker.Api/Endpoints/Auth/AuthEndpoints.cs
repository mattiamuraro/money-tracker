using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    private const long AuthRequestBodySizeLimitBytes = 4 * 1024;

    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        static void ApplyNoStoreHeaders(HttpResponse response)
        {
            response.Headers.CacheControl = "no-store, no-cache, max-age=0";
            response.Headers.Pragma = "no-cache";
            response.Headers.Expires = "0";
        }

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
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                };

                ApplyNoStoreHeaders(httpContext.Response);
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
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                };

                ApplyNoStoreHeaders(httpContext.Response);
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

        group.MapPost("/refresh", static async (HttpContext httpContext, [FromServices] IHandler<RefreshTokenCommand, RefreshAuthTokenDto> handler, RefreshTokenRequest request, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.ContentLength is > AuthRequestBodySizeLimitBytes)
                    return Results.Problem("Request payload is too large.", statusCode: StatusCodes.Status413PayloadTooLarge);

                var result = await handler.Handle(new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                }, cancellationToken);

                ApplyNoStoreHeaders(httpContext.Response);
                return Results.Ok(new AuthTokenResponse
                {
                    Token = result.Token,
                    RefreshToken = result.RefreshToken
                });
            })
            .WithName("Refresh")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/revoke", static async (HttpContext httpContext, [FromServices] IHandler<RevokeRefreshTokenCommand, bool> handler, RefreshTokenRequest request, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.ContentLength is > AuthRequestBodySizeLimitBytes)
                    return Results.Problem("Request payload is too large.", statusCode: StatusCodes.Status413PayloadTooLarge);

                await handler.Handle(new RevokeRefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                }, cancellationToken);

                ApplyNoStoreHeaders(httpContext.Response);
                return Results.NoContent();
            })
            .WithName("Revoke")
            .RequireRateLimiting("auth-login")
            .AllowAnonymous()
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/change-password", static async (HttpContext httpContext, [FromServices] IHandler<ChangePasswordCommand, bool> handler, ChangePasswordRequest request, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.ContentLength is > AuthRequestBodySizeLimitBytes)
                    return Results.Problem("Request payload is too large.", statusCode: StatusCodes.Status413PayloadTooLarge);

                var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                await handler.Handle(new ChangePasswordCommand
                {
                    UserId = userId,
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword
                }, cancellationToken);

                ApplyNoStoreHeaders(httpContext.Response);
                return Results.NoContent();
            })
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
