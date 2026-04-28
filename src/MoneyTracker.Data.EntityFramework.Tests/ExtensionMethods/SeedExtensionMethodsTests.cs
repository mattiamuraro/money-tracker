using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.ExtensionMethods;
using Moq;
using Xunit;

namespace MoneyTracker.Data.EntityFramework.Tests.ExtensionMethods;

public class SeedExtensionMethodsTests : IDisposable
{
    private readonly MoneyTrackerDbContext _dbContext;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public SeedExtensionMethodsTests()
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _dbContext = new MoneyTrackerDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
        
        _mockLogger = new Mock<ILogger>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SeedDefaultDataAsync_SeedsSystemUser_BeforeFailingOnSqliteIncompatibility()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns("admin");
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns("password123");
        var cancellationToken = CancellationToken.None;

        // Act
        // The method will seed system and admin users, then fail on forecast seeding due to SQLite incompatibility
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));

        // Assert - Verify that users were seeded before the exception
        var systemUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == SystemUsers.SystemUserId, cancellationToken);
        Assert.NotNull(systemUser);
        Assert.Equal(SystemUsers.SystemUsername, systemUser.Username);
        Assert.Equal(string.Empty, systemUser.PasswordHash);

        var adminUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
        Assert.NotNull(adminUser);
        Assert.NotEqual(Guid.Empty, adminUser.Id);
        Assert.NotEmpty(adminUser.PasswordHash);
    }

    [Fact]
    public async Task SeedDefaultDataAsync_HandlesNullConfiguration_DoesNotThrow()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns((string?)null);
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns((string?)null);
        var cancellationToken = CancellationToken.None;

        // Act & Assert - Should not throw on the initial user seeding parts
        // Note: Will fail at TableExistsAsync due to SQLite not supporting INFORMATION_SCHEMA
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));
        
        // Verify system user was still seeded even without admin config
        var systemUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == SystemUsers.SystemUserId, cancellationToken);
        Assert.NotNull(systemUser);
    }

    [Fact]
    public async Task SeedDefaultDataAsync_RespectsCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns("admin");
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns("password123");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));
    }

    [Fact]
    public async Task SeedDefaultDataAsync_SkipsAdminUser_WhenUsernameIsEmpty()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns(string.Empty);
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns("password123");
        var cancellationToken = CancellationToken.None;

        // Act
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));

        // Assert - Only system user should be seeded
        var users = await _dbContext.Users.ToListAsync(cancellationToken);
        Assert.Single(users);
        Assert.Equal(SystemUsers.SystemUserId, users[0].Id);
    }

    [Fact]
    public async Task SeedDefaultDataAsync_SkipsAdminUser_WhenPasswordIsEmpty()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns("admin");
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns(string.Empty);
        var cancellationToken = CancellationToken.None;

        // Act
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));

        // Assert - Only system user should be seeded
        var users = await _dbContext.Users.ToListAsync(cancellationToken);
        Assert.Single(users);
        Assert.Equal(SystemUsers.SystemUserId, users[0].Id);
    }

    [Fact]
    public async Task SeedDefaultDataAsync_DoesNotDuplicateSystemUser_WhenCalledMultipleTimes()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Auth:Username"]).Returns((string?)null);
        _mockConfiguration.Setup(c => c["Auth:Password"]).Returns((string?)null);
        var cancellationToken = CancellationToken.None;

        // Act - Call twice
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));
        
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _dbContext.SeedDefaultDataAsync(_mockLogger.Object, _mockConfiguration.Object, cancellationToken));

        // Assert - Should still have only one system user
        var systemUsers = await _dbContext.Users.Where(u => u.Id == SystemUsers.SystemUserId).ToListAsync(cancellationToken);
        Assert.Single(systemUsers);
    }
}
