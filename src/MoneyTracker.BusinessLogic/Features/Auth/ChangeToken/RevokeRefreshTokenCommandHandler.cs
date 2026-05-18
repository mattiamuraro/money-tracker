using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;

public class RevokeRefreshTokenCommandHandler(
    IValidator<RevokeRefreshTokenCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<RevokeRefreshTokenCommand, bool>
{
    public async Task<bool> Handle(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tokenHash = RefreshTokenHelper.ComputeTokenHash(command.RefreshToken);
        var now = DateTime.UtcNow;

        var refreshToken = await dbContext.UserRefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null)
            return true;

        if (!refreshToken.RevokedAt.HasValue)
            refreshToken.RevokedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
