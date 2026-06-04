using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.ExtensionMethods;

namespace MoneyTracker.Data.MigrationService;

public class MigrationWorker : BackgroundService
{
    private static readonly EventId WorkerStartingEventId = new(4001, nameof(WorkerStartingEventId));
    private static readonly EventId ApplyingMigrationsEventId = new(4002, nameof(ApplyingMigrationsEventId));
    private static readonly EventId MigrationsAppliedEventId = new(4003, nameof(MigrationsAppliedEventId));
    private static readonly EventId DefaultDataSeededEventId = new(4004, nameof(DefaultDataSeededEventId));
    private static readonly EventId WorkerCanceledEventId = new(4005, nameof(WorkerCanceledEventId));
    private static readonly EventId WorkerFailedEventId = new(4006, nameof(WorkerFailedEventId));
    private static readonly EventId WorkerCompletedEventId = new(4007, nameof(WorkerCompletedEventId));

    private static readonly Meter WorkerMeter = new("MoneyTracker.Workers");
    private static readonly Counter<long> WorkerRunsCounter = WorkerMeter.CreateCounter<long>("migration_worker_runs_total");
    private static readonly Counter<long> WorkerSucceededCounter = WorkerMeter.CreateCounter<long>("migration_worker_succeeded_total");
    private static readonly Counter<long> WorkerFailedCounter = WorkerMeter.CreateCounter<long>("migration_worker_failed_total");
    private static readonly Counter<long> WorkerCanceledCounter = WorkerMeter.CreateCounter<long>("migration_worker_canceled_total");

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationWorker> _logger;
    private readonly AdminCredentialsOptions _adminCredentials;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MigrationWorker(
        IServiceProvider serviceProvider,
        ILogger<MigrationWorker> logger,
        IOptions<AdminCredentialsOptions> adminCredentials,
        IHostApplicationLifetime applicationLifetime)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _adminCredentials = adminCredentials.Value;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runId = Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString();
        using var scope = _logger.BeginScope("RunId: {RunId}", runId);

        WorkerRunsCounter.Add(1);
        _logger.LogInformation(WorkerStartingEventId, "Starting EF migration worker...");

        try
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();

            _logger.LogInformation(ApplyingMigrationsEventId, "Applying EF Core migrations...");
            await db.Database.MigrateAsync(stoppingToken);
            _logger.LogInformation(MigrationsAppliedEventId, "Migrations applied successfully.");

            await db.SeedDefaultDataAsync(_logger, _adminCredentials, stoppingToken);

            _logger.LogInformation(DefaultDataSeededEventId, "Seeded default data.");
            WorkerSucceededCounter.Add(1);

            _logger.LogInformation(WorkerCompletedEventId, "Migration worker completed. Shutting down.");
            _applicationLifetime.StopApplication();
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            WorkerCanceledCounter.Add(1);
            _logger.LogInformation(WorkerCanceledEventId, "Migration worker was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            WorkerFailedCounter.Add(1);
            _logger.LogError(WorkerFailedEventId, ex, "Error while applying migrations.");
            throw;
        }
    }
}
