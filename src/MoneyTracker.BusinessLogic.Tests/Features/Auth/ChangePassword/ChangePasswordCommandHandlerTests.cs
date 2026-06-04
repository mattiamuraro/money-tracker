using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ChangePassword;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static PasswordPolicyOptions CreatePolicyOptions() => new()
    {
        MinimumLength = 12,
        MaximumLength = 128,
        PasswordHistoryCount = 3,
        BlockedPasswords = ["password123"]
    };

    private static ChangePasswordCommandHandler CreateHandler(MoneyTrackerDbContext db)
    {
        var passwordHasher = new PasswordHasher<User>();
        var options = Options.Create(CreatePolicyOptions());
        return new ChangePasswordCommandHandler(new ChangePasswordCommandValidator(options), passwordHasher, options, db);
    }

    [Fact]
    public async Task Handle_WithValidRequest_UpdatesPasswordAndHistory()
    {
        using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1"
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Current#Password1");

        db.Users.Add(user);
        db.UserPasswordHistories.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var oldHash = user.PasswordHash;
        var handler = CreateHandler(db);

        var result = await handler.Handle(new ChangePasswordCommand
        {
            UserId = user.Id,
            CurrentPassword = "Current#Password1",
            NewPassword = "New#Password123"
        }, CancellationToken.None);

        Assert.True(result);

        var savedUser = await db.Users.SingleAsync();
        Assert.NotEqual(oldHash, savedUser.PasswordHash);
        Assert.Equal(2, await db.UserPasswordHistories.CountAsync());
    }

    [Fact]
    public async Task Handle_WithWrongCurrentPassword_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1"
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Current#Password1");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new ChangePasswordCommand
            {
                UserId = user.Id,
                CurrentPassword = "wrong-password",
                NewPassword = "New#Password123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRecentPasswordReuse_ThrowsValidationException()
    {
        using var db = CreateDbContext();
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1"
        };

        var currentHash = passwordHasher.HashPassword(user, "Current#Password1");
        var reusedHash = passwordHasher.HashPassword(user, "Reused#Password1");
        user.PasswordHash = currentHash;

        db.Users.Add(user);
        db.UserPasswordHistories.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = currentHash,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        db.UserPasswordHistories.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = reusedHash,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new ChangePasswordCommand
            {
                UserId = user.Id,
                CurrentPassword = "Current#Password1",
                NewPassword = "Reused#Password1"
            }, CancellationToken.None));
    }
}
