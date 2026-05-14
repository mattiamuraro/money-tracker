using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.Login;

public class LoginCommandHandler(
    IValidator<LoginCommand> validator,
    IOptions<JwtOptions> jwtOptions,
    IPasswordHasher<User> passwordHasher,
    MoneyTrackerDbContext dbContext)
    : IHandler<LoginCommand, LoginAuthTokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginAuthTokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == command.Username, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Username or password is incorrect");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Username or password is incorrect");

        var token = user.BuildToken(_jwtOptions);

        return new LoginAuthTokenDto { Token = token };
    }
}
