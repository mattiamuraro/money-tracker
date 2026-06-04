using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;

public class RefreshTokenCommandHandler(
    IValidator<RefreshTokenCommand> validator,
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    TimeProvider timeProvider,
    MoneyTrackerDbContext dbContext)
    : IHandler<RefreshTokenCommand, RefreshAuthTokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task<RefreshAuthTokenDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tokenHash = RefreshTokenHelper.ComputeTokenHash(command.RefreshToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var refreshToken = await dbContext.UserRefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException(Login.LoginCommand.AuthFailedMessage);

        if (refreshToken.RevokedAt.HasValue || refreshToken.ExpiresAt <= now)
            throw new UnauthorizedAccessException(Login.LoginCommand.AuthFailedMessage);

        var rotatedToken = RefreshTokenHelper.GenerateRefreshToken();
        var rotatedTokenHash = RefreshTokenHelper.ComputeTokenHash(rotatedToken);

        refreshToken.RevokedAt = now;
        refreshToken.ReplacedByTokenHash = rotatedTokenHash;

        dbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = refreshToken.UserId,
            TokenHash = rotatedTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_refreshTokenOptions.ExpiryDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshAuthTokenDto
        {
            Token = refreshToken.User.BuildToken(_jwtOptions),
            RefreshToken = rotatedToken
        };
    }
}
