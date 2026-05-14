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
    ILoginAttemptService loginAttemptService,
    MoneyTrackerDbContext dbContext)
    : IHandler<LoginCommand, LoginAuthTokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginAuthTokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var username = command.Username.Trim();
        var nowUtc = DateTimeOffset.UtcNow;

        if (loginAttemptService.IsLockedOut(username, nowUtc))
            throw new UnauthorizedAccessException(LoginCommand.AuthFailedMessage);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null)
        {
            loginAttemptService.RegisterFailure(username, nowUtc);
            throw new UnauthorizedAccessException(LoginCommand.AuthFailedMessage);
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            loginAttemptService.RegisterFailure(username, nowUtc);
            throw new UnauthorizedAccessException(LoginCommand.AuthFailedMessage);
        }

        loginAttemptService.RegisterSuccess(username);

        var token = user.BuildToken(_jwtOptions);

        return new LoginAuthTokenDto { Token = token };
    }
}
