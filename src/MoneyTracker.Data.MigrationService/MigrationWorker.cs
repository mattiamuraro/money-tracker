using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.ExtensionMethods;

namespace MoneyTracker.Data.MigrationService;

public class MigrationWorker : IHostedService
{
    private static readonly EventId WorkerStartingEventId = new(4001, nameof(WorkerStartingEventId));
    private static readonly EventId ApplyingMigrationsEventId = new(4002, nameof(ApplyingMigrationsEventId));
    private static readonly EventId MigrationsAppliedEventId = new(4003, nameof(MigrationsAppliedEventId));
    private static readonly EventId DefaultDataSeededEventId = new(4004, nameof(DefaultDataSeededEventId));
    private static readonly EventId WorkerCanceledEventId = new(4005, nameof(WorkerCanceledEventId));
    private static readonly EventId WorkerFailedEventId = new(4006, nameof(WorkerFailedEventId));
    private static readonly EventId WorkerCompletedEventId = new(4007, nameof(WorkerCompletedEventId));

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IConfiguration _configuration;

    public MigrationWorker(IServiceProvider serviceProvider, ILogger<MigrationWorker> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(WorkerStartingEventId, "Starting EF migration worker...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();

            _logger.LogInformation(ApplyingMigrationsEventId, "Applying EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation(MigrationsAppliedEventId, "Migrations applied successfully.");

            await db.SeedDefaultDataAsync(_logger, _configuration, cancellationToken);

            _logger.LogInformation(DefaultDataSeededEventId, "Seeded default data.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(WorkerCanceledEventId, "Migration worker was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(WorkerFailedEventId, ex, "Error while applying migrations.");
            throw;
        }

        _logger.LogInformation(WorkerCompletedEventId, "Migration worker completed. Shutting down.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
