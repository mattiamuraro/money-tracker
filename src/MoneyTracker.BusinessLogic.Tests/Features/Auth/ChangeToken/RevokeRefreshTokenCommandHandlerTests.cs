using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.Security.Cryptography;
using System.Text;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ChangeToken;

public class RevokeRefreshTokenCommandHandlerTests
{
    private static string ComputeTokenHash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    [Fact]
    public async Task Handle_WithExistingToken_RevokesToken()
    {
        using var db = CreateDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            PasswordHash = "hash"
        };

        var refreshToken = "revoke-me";
        db.Users.Add(user);
        db.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeTokenHash(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        });
        await db.SaveChangesAsync();

        var handler = new RevokeRefreshTokenCommandHandler(new RevokeRefreshTokenCommandValidator(), db);

        var result = await handler.Handle(new RevokeRefreshTokenCommand { RefreshToken = refreshToken }, CancellationToken.None);

        Assert.True(result);
        var tokenEntity = await db.UserRefreshTokens.SingleAsync();
        Assert.NotNull(tokenEntity.RevokedAt);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsTrue()
    {
        using var db = CreateDbContext();
        var handler = new RevokeRefreshTokenCommandHandler(new RevokeRefreshTokenCommandValidator(), db);

        var result = await handler.Handle(new RevokeRefreshTokenCommand { RefreshToken = "unknown" }, CancellationToken.None);

        Assert.True(result);
    }
}
