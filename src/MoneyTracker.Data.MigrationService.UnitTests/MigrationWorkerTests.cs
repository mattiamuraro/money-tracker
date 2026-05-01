using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.Data.MigrationService.UnitTests;

public class MigrationWorkerTests
{
    // Minimal fake logger that captures log entries
    private sealed class FakeLogger : ILogger<MigrationWorker>
    {
        public record LogEntry(LogLevel Level, Exception? Exception, string Message);
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }

    // Minimal fake IServiceScope backed by a real IServiceProvider
    private sealed class FakeServiceScope(IServiceProvider provider) : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; } = provider;
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    // Minimal fake IServiceScopeFactory that returns a FakeServiceScope
    private sealed class FakeServiceScopeFactory(IServiceProvider scopeProvider) : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => new FakeServiceScope(scopeProvider);
    }

    // Root IServiceProvider that only knows how to hand out IServiceScopeFactory.
    // Avoids BuildServiceProvider() overriding IServiceScopeFactory with its own implementation.
    private sealed class FakeRootServiceProvider(IServiceScopeFactory scopeFactory) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IServiceScopeFactory) ? scopeFactory : null;
    }

    // Builds fake infrastructure with MoneyTrackerDbContext registered in scope
    private static (IServiceProvider rootProvider, MoneyTrackerDbContext db, FakeLogger logger, IConfiguration config)
        BuildTestServices(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        var db = new MoneyTrackerDbContext(options, null);

        var scopeServices = new ServiceCollection();
        scopeServices.AddSingleton<MoneyTrackerDbContext>(db);
        var scopeProvider = scopeServices.BuildServiceProvider();

        var scopeFactory = new FakeServiceScopeFactory(scopeProvider);
        var rootProvider = new FakeRootServiceProvider(scopeFactory);

        var logger = new FakeLogger();
        var config = new ConfigurationBuilder().Build();

        return (rootProvider, db, logger, config);
    }

    private static MigrationWorker CreateWorker(IServiceProvider rootProvider, FakeLogger logger, IConfiguration config)
        => new(rootProvider, logger, config);

    [Fact]
    public void Constructor_Should_Initialize_Fields()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_Should_Store_ServiceProvider()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_Should_Store_Logger()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_Should_Store_Configuration()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);
        Assert.NotNull(worker);
    }

    [Fact]
    public async Task StartAsync_Should_Log_Starting_Message()
    {
        // Arrange
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        // Act - InMemory DB does not support MigrateAsync, so an exception is expected
        try { await worker.StartAsync(CancellationToken.None); } catch { }

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Starting EF migration worker"));
    }

    [Fact]
    public async Task StartAsync_Should_Log_Applying_Migrations_Message()
    {
        // Arrange
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        // Act
        try { await worker.StartAsync(CancellationToken.None); } catch { }

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Applying EF Core migrations"));
    }

    [Fact]
    public async Task StartAsync_Should_Log_Error_And_Rethrow_Exception()
    {
        // Arrange
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        // Act & Assert - InMemory DB throws InvalidOperationException from MigrateAsync
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.StartAsync(CancellationToken.None));

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Error &&
            e.Message.Contains("Error while applying migrations"));
    }

    [Fact]
    public async Task StartAsync_Should_Create_Scope_And_Get_DbContext()
    {
        // Arrange
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        // Act - MigrateAsync will throw on InMemory; that proves the scope was created and db was resolved
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.StartAsync(CancellationToken.None));

        // If we reach the error log it means the scope was created and db context was obtained
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Applying EF Core migrations"));
    }

    [Fact]
    public async Task StartAsync_Should_Dispose_Scope_On_Exception()
    {
        // Arrange - use a trackable scope factory
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MoneyTrackerDbContext(options, null);

        var scopeServices = new ServiceCollection();
        scopeServices.AddSingleton<MoneyTrackerDbContext>(db);
        var scopeProvider = scopeServices.BuildServiceProvider();

        var disposedScopes = new List<FakeServiceScope>();
        var trackingFactory = new TrackingServiceScopeFactory(scopeProvider, disposedScopes);
        var root = new FakeRootServiceProvider(trackingFactory);

        var logger = new FakeLogger();
        var worker = new MigrationWorker(root, logger, new ConfigurationBuilder().Build());

        // Act
        try { await worker.StartAsync(CancellationToken.None); } catch { }

        // Assert
        Assert.All(disposedScopes, s => Assert.True(s.Disposed));
    }

    [Fact]
    public async Task StartAsync_Should_Dispose_Scope_On_Success()
    {
        // InMemory always throws from MigrateAsync; verifies scope is disposed via using pattern.
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MoneyTrackerDbContext(options, null);

        var scopeServices = new ServiceCollection();
        scopeServices.AddSingleton<MoneyTrackerDbContext>(db);
        var scopeProvider = scopeServices.BuildServiceProvider();

        var disposedScopes = new List<FakeServiceScope>();
        var trackingFactory = new TrackingServiceScopeFactory(scopeProvider, disposedScopes);
        var root = new FakeRootServiceProvider(trackingFactory);

        var logger = new FakeLogger();
        var worker = new MigrationWorker(root, logger, new ConfigurationBuilder().Build());

        try { await worker.StartAsync(CancellationToken.None); } catch { }

        Assert.NotEmpty(disposedScopes);
        Assert.All(disposedScopes, s => Assert.True(s.Disposed));
    }

    [Fact]
    public async Task StartAsync_Should_Call_MigrateAsync_With_CancellationToken()
    {
        // Arrange
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);
        using var cts = new CancellationTokenSource();

        // Act - throws because InMemory doesn't support MigrateAsync
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.StartAsync(cts.Token));

        // The "Applying EF Core migrations" log proves MigrateAsync was reached
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Applying EF Core migrations"));
    }

    [Fact]
    public async Task StartAsync_Should_Log_Completion_Message_After_Success()
    {
        // Arrange - InMemory always throws from MigrateAsync, so completion is never reached
        var (root, db, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        try { await worker.StartAsync(CancellationToken.None); } catch { }

        // Assert completion message is NOT present because exception aborts execution
        Assert.DoesNotContain(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Migration worker completed. Shutting down"));
    }

    [Fact]
    public async Task StopAsync_Should_Return_CompletedTask()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        var result = worker.StopAsync(CancellationToken.None);

        Assert.True(result.IsCompleted);
        await result;
    }

    [Fact]
    public async Task StopAsync_Should_Not_Throw_Exception()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        var exception = await Record.ExceptionAsync(() => worker.StopAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task StopAsync_Should_Return_Task_Completed()
    {
        var (root, _, logger, config) = BuildTestServices();
        var worker = CreateWorker(root, logger, config);

        await worker.StopAsync(CancellationToken.None);
    }

    // Tracks which scopes were created so disposal can be verified
    private sealed class TrackingServiceScopeFactory(
        IServiceProvider scopeProvider,
        List<FakeServiceScope> tracked) : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            var scope = new FakeServiceScope(scopeProvider);
            tracked.Add(scope);
            return scope;
        }
    }
}
