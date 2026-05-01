using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.Api.Endpoints.Auth.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
                    .WithTags("Auth");

        group.MapPost("/login", async ([FromServices] LoginCommandHandler handler, LoginRequest request, CancellationToken cancellationToken) =>
            {
                var command = request.ToLoginCommand();
                var result = await handler.Handle(command, cancellationToken);
                var response = result.ToLoginAuthTokenResponse();

                return Results.Ok(response);
            })
            .WithName("Login")
            .AllowAnonymous()
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", async ([FromServices] RegisterCommandHandler handler, RegisterRequest request, CancellationToken cancellationToken) =>
            {
                var command = request.ToRegisterCommand();
                var result = await handler.Handle(command, cancellationToken);
                var response = result.ToRegisterAuthTokenResponse();

                return Results.Ok(response);
            })
            .WithName("Register")
            .AllowAnonymous()
            .Accepts<RegisterRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapGet("/config", ([FromServices] IOptions<AuthOptions> authOptions) =>
                Results.Ok(new AuthConfigResponse { AllowRegistration = authOptions.Value.AllowRegistration }))
            .WithName("GetAuthConfig")
            .AllowAnonymous()
            .Produces<AuthConfigResponse>(StatusCodes.Status200OK);

        return app;
    }
}
