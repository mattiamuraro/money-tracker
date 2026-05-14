using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Login;

public class LoginCommandHandlerTests
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

    private static LoginCommandHandler CreateHandler(
        MoneyTrackerDbContext db,
        PasswordVerificationResult verificationResult = PasswordVerificationResult.Success,
        ILoginAttemptService? loginAttemptService = null) =>
        new(new LoginCommandValidator(),
            Options.Create(DefaultJwtOptions),
            new FakePasswordHasher(verificationResult),
            loginAttemptService ?? new FakeLoginAttemptService(),
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
    public async Task Handle_ValidCredentials_ReturnsLoginAuthToken()
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

        var handler = CreateHandler(db, PasswordVerificationResult.Success);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new LoginCommand { Username = "", Password = "password123" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new LoginCommand { Username = "nonexistentuser", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Authentication failed.", exception.Message);
    }

    [Fact]
    public async Task Handle_PasswordVerificationFailed_ThrowsUnauthorizedAccessException()
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

        var handler = CreateHandler(db, PasswordVerificationResult.Failed);
        var command = new LoginCommand { Username = "testuser", Password = "wrongpassword" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Authentication failed.", exception.Message);
    }

    [Fact]
    public async Task Handle_PasswordVerificationSuccessRehashNeeded_ReturnsLoginAuthToken()
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

        var handler = CreateHandler(db, PasswordVerificationResult.SuccessRehashNeeded);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var db = CreateDbContext();
        var handler = CreateHandler(db);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenUsernameLockedOut_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var db = CreateDbContext();
        var lockedOutAttempts = new FakeLoginAttemptService(isLockedOut: true);
        var handler = CreateHandler(db, PasswordVerificationResult.Success, lockedOutAttempts);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Authentication failed.", exception.Message);
    }

    private sealed class FakePasswordHasher : IPasswordHasher<User>
    {
        private readonly PasswordVerificationResult _verificationResult;

        public FakePasswordHasher(PasswordVerificationResult verificationResult) =>
            _verificationResult = verificationResult;

        public string HashPassword(User user, string password) => password;

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword) =>
            _verificationResult;
    }

    private sealed class FakeLoginAttemptService(bool isLockedOut = false) : ILoginAttemptService
    {
        public bool IsLockedOut(string username, DateTimeOffset nowUtc) => isLockedOut;

        public void RegisterFailure(string username, DateTimeOffset nowUtc)
        {
        }

        public void RegisterSuccess(string username)
        {
        }
    }
}

