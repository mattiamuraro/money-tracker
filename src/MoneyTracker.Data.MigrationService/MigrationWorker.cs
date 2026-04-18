using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.EntityFramework.ExtensionMethods;

namespace MoneyTracker.Data.MigrationService;

public class MigrationWorker : IHostedService
{
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
        _logger.LogInformation("Starting EF migration worker...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MoneyTrackerDbContext>();

            _logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations applied successfully.");

            await db.SeedDefaultDataAsync(_logger, _configuration, cancellationToken);

            _logger.LogInformation("Seeded default data.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while applying migrations.");
            throw;
        }

        _logger.LogInformation("Migration worker completed. Shutting down.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
