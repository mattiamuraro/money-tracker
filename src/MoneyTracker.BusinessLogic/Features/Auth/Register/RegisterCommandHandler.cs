using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.Register;

public class RegisterCommandHandler(
    IValidator<RegisterCommand> validator,
    IOptions<AuthOptions> authOptions,
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IPasswordHasher<User> passwordHasher,
    MoneyTrackerDbContext dbContext)
    : IHandler<RegisterCommand, RegisterAuthTokenDto>
{
    private readonly AuthOptions _authOptions = authOptions.Value;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task<RegisterAuthTokenDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (!_authOptions.AllowRegistration)
            throw new UnauthorizedAccessException("Registration is currently disabled.");

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var username = command.Username.Trim();
        var usernameTaken = await dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);
        if (usernameTaken)
            throw new ValidationException("Username is already taken.");

        var passwordHash = passwordHasher.HashPassword(null!, command.Password);
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.UserPasswordHistories.Add(new UserPasswordHistory
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        });

        var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
        var refreshTokenHash = RefreshTokenHelper.ComputeTokenHash(refreshToken);

        dbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenOptions.ExpiryDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var token = user.BuildToken(_jwtOptions);
        return new RegisterAuthTokenDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }
}
