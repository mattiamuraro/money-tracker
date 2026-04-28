using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Register;

public class RegisterCommandHandlerTests
{
    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = true
        });

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        // Act
        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_RegistrationDisabled_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = false
        });

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Registration is currently disabled.", exception.Message);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = true
        });

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Validation failed"));

        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        var command = new RegisterCommand
        {
            Username = "",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UsernameTaken_ThrowsValidationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = true
        });

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Username is already taken.", exception.Message);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        var jwtOptions = new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = true
        });

        mockJwtOptions.Setup(x => x.Value).Returns(jwtOptions);

        mockValidator
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockPasswordHasher
            .Setup(h => h.HashPassword(null!, "password123"))
            .Returns("hashed_password123");

        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new RegisterCommand
        {
            Username = "newuser",
            Password = "password123"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);

        var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.NotNull(savedUser);
        Assert.Equal("newuser", savedUser.Username);
        Assert.Equal("hashed_password123", savedUser.PasswordHash);
        Assert.NotEqual(Guid.Empty, savedUser.Id);
        Assert.NotEqual(default(DateTime), savedUser.CreatedAt);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<RegisterCommand>>();
        var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockAuthOptions.Setup(x => x.Value).Returns(new AuthOptions
        {
            AllowRegistration = true
        });

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var handler = new RegisterCommandHandler(
            mockValidator.Object,
            mockAuthOptions.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        var command = new RegisterCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cancellationTokenSource.Token));
    }
}
