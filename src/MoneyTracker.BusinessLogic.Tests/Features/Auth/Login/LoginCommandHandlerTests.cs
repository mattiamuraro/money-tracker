using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.Features.Auth.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public void Constructor_ShouldInitialize_AllDependencies()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        // Act
        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginAuthToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        var jwtOptions = new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };
        mockJwtOptions.Setup(x => x.Value).Returns(jwtOptions);

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mockPasswordHasher
            .Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.Success);

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        var validationFailure = new ValidationFailure("Username", "Username is required");
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        var command = new LoginCommand
        {
            Username = "",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new LoginCommand
        {
            Username = "nonexistentuser",
            Password = "password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Username or password is incorrect", exception.Message);
    }

    [Fact]
    public async Task Handle_PasswordVerificationFailed_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mockPasswordHasher
            .Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "wrongpassword"))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Username or password is incorrect", exception.Message);
    }

    [Fact]
    public async Task Handle_PasswordVerificationSuccessRehashNeeded_ReturnsLoginAuthToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var dbContext = new MoneyTrackerDbContext(options, null!);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

        var jwtOptions = new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };
        mockJwtOptions.Setup(x => x.Value).Returns(jwtOptions);

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mockPasswordHasher
            .Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            dbContext);

        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<LoginCommand>>();
        var mockJwtOptions = new Mock<IOptions<JwtOptions>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockDbContext = new Mock<MoneyTrackerDbContext>(
            new DbContextOptions<MoneyTrackerDbContext>(),
            null!);

        mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
        {
            Key = "test-key-with-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var handler = new LoginCommandHandler(
            mockValidator.Object,
            mockJwtOptions.Object,
            mockPasswordHasher.Object,
            mockDbContext.Object);

        var command = new LoginCommand
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
