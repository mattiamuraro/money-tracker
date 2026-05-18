using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Register;

public class RegisterCommandHandlerTests
{
    private static readonly JwtOptions DefaultJwtOptions = new()
    {
        Key = "test-key-with-at-least-32-characters-long",
        Issuer = "test-issuer",
        Audience = "test-audience"
    };

    private static MoneyTrackerDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options, null);

    private static RegisterCommandHandler CreateHandler(MoneyTrackerDbContext db, bool allowRegistration = true) =>
        new(new RegisterCommandValidator(),
            Options.Create(new AuthOptions { AllowRegistration = allowRegistration }),
            Options.Create(DefaultJwtOptions),
            Options.Create(new RefreshTokenOptions { ExpiryDays = 14 }),
            new FakePasswordHasher(),
            db);

    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        using var db = CreateDbContext();

        // Act
        var handler = CreateHandler(db);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_RegistrationDisabled_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db, allowRegistration: false);
        var command = new RegisterCommand { Username = "testuser", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Registration is currently disabled.", exception.Message);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new RegisterCommand { Username = "", Password = "password123" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UsernameTaken_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var command = new RegisterCommand { Username = "testuser", Password = "StrongPass#123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Username is already taken.", exception.Message);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsToken()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new RegisterCommand { Username = "newuser", Password = "StrongPass#123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);

        var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.NotNull(savedUser);
        Assert.Equal("newuser", savedUser.Username);
        Assert.Equal("hashed_StrongPass#123", savedUser.PasswordHash);
        Assert.NotEqual(Guid.Empty, savedUser.Id);
        Assert.NotEqual(default(DateTime), savedUser.CreatedAt);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new RegisterCommand { Username = "testuser", Password = "password123" };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }

    private sealed class FakePasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password) => $"hashed_{password}";

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword) =>
            hashedPassword == HashPassword(user, providedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
    }
}
