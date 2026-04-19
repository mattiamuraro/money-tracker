using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.Api.Endpoints.Auth.Services;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.Api.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    internal static WebApplication AddAuthApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
                    .WithTags("Auth");

        group.MapPost("/login", async ([FromServices] JwtTokenService tokenService, [FromServices] IValidator<LoginCommand> validator, LoginRequest request, CancellationToken cancellationToken) =>
            {
                try
                {
                    var command = new LoginCommand
                    {
                        Username = request.Username,
                        Password = request.Password
                    };

                    await validator.ValidateAndThrowAsync(command, cancellationToken);
                    var result = await tokenService.LoginAsync(command.Username, command.Password, cancellationToken);
                    return result switch
                    {
                        null => Results.Unauthorized(),
                        _ => Results.Ok(new AuthTokenResponse { Token = result })
                    };
                }
                catch (ValidationException ex)
                {
                    return Results.ValidationProblem(ex.ToValidationErrors());
                }
            })
            .WithName("Login")
            .AllowAnonymous()
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", async ([FromServices] JwtTokenService tokenService, [FromServices] IValidator<RegisterCommand> validator, [FromServices] IOptions<AuthOptions> authOptions, RegisterRequest request, CancellationToken cancellationToken) =>
            {
                if (!authOptions.Value.AllowRegistration)
                    return Results.StatusCode(StatusCodes.Status403Forbidden);

                try
                {
                    var command = new RegisterCommand
                    {
                        Username = request.Username,
                        Password = request.Password
                    };

                    await validator.ValidateAndThrowAsync(command, cancellationToken);
                    var token = await tokenService.RegisterAsync(command.Username, command.Password, cancellationToken);
                    return token switch
                    {
                        null => Results.Conflict(new ErrorResponse { Message = "Username is already taken.", StatusCode = 409 }),
                        _ => Results.Ok(new AuthTokenResponse { Token = token })
                    };
                }
                catch (ValidationException ex)
                {
                    return Results.ValidationProblem(ex.ToValidationErrors());
                }
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
