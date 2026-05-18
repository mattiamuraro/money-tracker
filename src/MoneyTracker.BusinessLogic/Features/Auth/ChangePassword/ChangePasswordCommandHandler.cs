using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    IValidator<ChangePasswordCommand> validator,
    IPasswordHasher<User> passwordHasher,
    IOptions<PasswordPolicyOptions> passwordPolicyOptions,
    MoneyTrackerDbContext dbContext)
    : IHandler<ChangePasswordCommand, bool>
{
    private readonly PasswordPolicyOptions _passwordPolicyOptions = passwordPolicyOptions.Value;

    public async Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException(Login.LoginCommand.AuthFailedMessage);

        var currentPasswordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.CurrentPassword);
        if (currentPasswordResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException(Login.LoginCommand.AuthFailedMessage);

        var recentPasswords = await dbContext.UserPasswordHistories
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(_passwordPolicyOptions.PasswordHistoryCount)
            .Select(x => x.PasswordHash)
            .ToListAsync(cancellationToken);

        var reused = recentPasswords.Any(hash =>
            passwordHasher.VerifyHashedPassword(user, hash, command.NewPassword) != PasswordVerificationResult.Failed);
        if (reused)
            throw new ValidationException("Password was used recently. Choose a different password.");

        user.PasswordHash = passwordHasher.HashPassword(user, command.NewPassword);

        dbContext.UserPasswordHistories.Add(new UserPasswordHistory
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
