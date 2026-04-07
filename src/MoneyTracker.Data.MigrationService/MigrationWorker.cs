using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;
namespace MoneyTracker.Data.MigrationService;

public class MigrationWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationWorker> _logger;

    public MigrationWorker(IServiceProvider serviceProvider, ILogger<MigrationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting EF migration worker...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();

            _logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while applying migrations.");
            throw;
        }

        // The worker shuts down immediately after migration
        _logger.LogInformation("Migration worker completed. Shutting down.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do: the worker terminates right after StartAsync
        return Task.CompletedTask;
    }
}
