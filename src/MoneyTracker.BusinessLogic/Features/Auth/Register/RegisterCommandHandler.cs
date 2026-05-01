using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.Register;

public class RegisterCommandHandler(
    IValidator<RegisterCommand> validator,
    IOptions<AuthOptions> authOptions,
    IOptions<JwtOptions> jwtOptions,
    IPasswordHasher<User> passwordHasher,
    MoneyTrackerDbContext dbContext)
    : IHandler<RegisterCommand, RegisterAuthToken>
{
    private readonly AuthOptions _authOptions = authOptions.Value;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RegisterAuthToken> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (!_authOptions.AllowRegistration)
            throw new UnauthorizedAccessException("Registration is currently disabled.");

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var usernameTaken = await dbContext.Users.AnyAsync(u => u.Username == command.Username, cancellationToken);
        if (usernameTaken)
            throw new ValidationException("Username is already taken.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = command.Username,
            PasswordHash = passwordHasher.HashPassword(null!, command.Password),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = user.BuildToken(_jwtOptions);
        return new RegisterAuthToken { Token = token };
    }
}
