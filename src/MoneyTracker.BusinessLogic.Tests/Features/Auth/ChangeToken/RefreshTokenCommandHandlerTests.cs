using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.Security.Cryptography;
using System.Text;
namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.ChangeToken;

public class RefreshTokenCommandHandlerTests
{
    private static readonly JwtOptions DefaultJwtOptions = new()
    {
        Key = "test-key-with-at-least-32-characters-long",
        Issuer = "test-issuer",
        Audience = "test-audience"
    };

    private static string ComputeTokenHash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static RefreshTokenCommandHandler CreateHandler(MoneyTrackerDbContext db) =>
        new(new RefreshTokenCommandValidator(),
            Options.Create(DefaultJwtOptions),
            Options.Create(new RefreshTokenOptions { ExpiryDays = 14 }),
            db);

    [Fact]
    public async Task Handle_WithValidRefreshToken_RotatesToken()
    {
        using var db = CreateDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            PasswordHash = "hash"
        };

        var refreshToken = "valid-refresh-token";
        db.Users.Add(user);
        db.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeTokenHash(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.Handle(new RefreshTokenCommand { RefreshToken = refreshToken }, CancellationToken.None);

        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);

        var tokens = await db.UserRefreshTokens.Where(x => x.UserId == user.Id).ToListAsync();
        Assert.Equal(2, tokens.Count);
        Assert.Single(tokens.Where(x => x.RevokedAt is null));
        Assert.Single(tokens.Where(x => x.RevokedAt is not null));
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            PasswordHash = "hash"
        };

        var refreshToken = "expired-refresh-token";
        db.Users.Add(user);
        db.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeTokenHash(refreshToken),
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RefreshTokenCommand { RefreshToken = refreshToken }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownRefreshToken_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateDbContext();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RefreshTokenCommand { RefreshToken = "missing-token" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRevokedRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange — seed a valid token then revoke it
        using var db = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), Username = "user1", PasswordHash = "hash" };
        var rawToken = "token-to-revoke";
        db.Users.Add(user);
        db.UserRefreshTokens.Add(new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeTokenHash(rawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await db.SaveChangesAsync();

        var revokeHandler = new RevokeRefreshTokenCommandHandler(new RevokeRefreshTokenCommandValidator(), db);
        await revokeHandler.Handle(new RevokeRefreshTokenCommand { RefreshToken = rawToken }, CancellationToken.None);

        // Act & Assert — revoked token must not produce a new access token
        var refreshHandler = CreateHandler(db);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            refreshHandler.Handle(new RefreshTokenCommand { RefreshToken = rawToken }, CancellationToken.None));
    }
}
