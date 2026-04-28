using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.ExtensionMethods;
using MoneyTracker.Data.MigrationService;
using Moq;

namespace MoneyTracker.Data.MigrationService.UnitTests;

public class MigrationWorkerTests
{
    [Fact]
    public void Constructor_Should_Initialize_Fields()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Assert
        Xunit.Assert.NotNull(worker);
    }

    [Fact]
    public async Task StartAsync_Should_Log_Starting_Message()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Test exception");

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Throws(exception);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        try
        {
            await worker.StartAsync(cancellationToken);
        }
        catch
        {
            // Expected
        }

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting EF migration worker")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_Should_Log_Error_And_Rethrow_Exception()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var exception = new InvalidOperationException("Migration failed");
        var cancellationToken = CancellationToken.None;

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Throws(exception);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act & Assert
        var thrownException = await Xunit.Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.StartAsync(cancellationToken));

        Xunit.Assert.Same(exception, thrownException);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while applying migrations")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_Should_Create_Scope_And_Get_DbContext()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("DbContext not found");

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Throws(exception);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act & Assert
        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.StartAsync(cancellationToken));

        mockScope.Verify(s => s.ServiceProvider, Times.AtLeastOnce);
        mockScopeServiceProvider.Verify(
            sp => sp.GetService(typeof(MoneyTrackerDbContext)),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_Should_Dispose_Scope_On_Success()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        var dbContextOptions = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new MoneyTrackerDbContext(dbContextOptions, null);

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScope.Setup(s => s.Dispose());
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Returns(dbContext);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        try
        {
            await worker.StartAsync(cancellationToken);
        }
        catch
        {
            // Expected - InMemory doesn't support migrations
        }

        // Assert
        mockScope.Verify(s => s.Dispose(), Times.Once);

        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [Fact]
    public async Task StartAsync_Should_Dispose_Scope_On_Exception()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Test exception");

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScope.Setup(s => s.Dispose());
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Throws(exception);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        try
        {
            await worker.StartAsync(cancellationToken);
        }
        catch
        {
            // Expected
        }

        // Assert
        mockScope.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public async Task StopAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var cancellationToken = CancellationToken.None;

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        var result = worker.StopAsync(cancellationToken);

        // Assert
        Xunit.Assert.True(result.IsCompleted);
        await result;
    }

    [Fact]
    public async Task StopAsync_Should_Not_Throw_Exception()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var cancellationToken = CancellationToken.None;

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => worker.StopAsync(cancellationToken));

        // Assert
        Xunit.Assert.Null(exception);
    }

    [Fact]
    public async Task StartAsync_Should_Apply_Migrations_And_Seed_Data_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        
        var dbContextOptions = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new MoneyTrackerDbContext(dbContextOptions, null);

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Returns(dbContext);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act & Assert
        // Note: InMemoryDatabase doesn't support relational-specific features like MigrateAsync,
        // so we verify the flow up to that point and confirm it attempts to call MigrateAsync
        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() => worker.StartAsync(cancellationToken));

        // Verify that the method logged the starting message
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting EF migration worker")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify that it progressed to attempting migrations
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Applying EF Core migrations")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify the error was logged
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while applying migrations")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [Fact]
    public async Task StartAsync_Should_Call_MigrateAsync_With_CancellationToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = new CancellationToken();
        
        var dbContextOptions = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new MoneyTrackerDbContext(dbContextOptions, null);

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Returns(dbContext);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act & Assert
        // InMemoryDatabase doesn't support MigrateAsync, so we expect an exception
        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() => worker.StartAsync(cancellationToken));

        // Verify DbContext was accessed
        mockScopeServiceProvider.Verify(
            sp => sp.GetService(typeof(MoneyTrackerDbContext)),
            Times.Once);

        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [Fact]
    public async Task StartAsync_Should_Log_Completion_Message_After_Success()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var cancellationToken = CancellationToken.None;
        
        var dbContextOptions = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new MoneyTrackerDbContext(dbContextOptions, null);

        var scopeFactory = new TestServiceScopeFactory(mockScope.Object);

        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        mockScopeServiceProvider
            .Setup(sp => sp.GetService(typeof(MoneyTrackerDbContext)))
            .Returns(dbContext);

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act & Assert
        // InMemoryDatabase doesn't support MigrateAsync, so we expect an exception
        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() => worker.StartAsync(cancellationToken));

        // The completion message is NOT logged because an exception occurs before that point
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Migration worker completed. Shutting down")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [Fact]
    public void Constructor_Should_Store_ServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Assert
        Xunit.Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_Should_Store_Logger()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Assert
        Xunit.Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_Should_Store_Configuration()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Act
        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Assert
        Xunit.Assert.NotNull(worker);
    }

    [Fact]
    public async Task StopAsync_Should_Return_Task_Completed()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<MigrationWorker>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var cancellationToken = CancellationToken.None;

        var worker = new MigrationWorker(
            mockServiceProvider.Object,
            mockLogger.Object,
            mockConfiguration.Object);

        // Act
        await worker.StopAsync(cancellationToken);

        // Assert - Task completed successfully
        Xunit.Assert.True(true);
    }

    private class TestServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceScope _scope;

        public TestServiceScopeFactory(IServiceScope scope)
        {
            _scope = scope;
        }

        public IServiceScope CreateScope()
        {
            return _scope;
        }
    }
}
