using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.Api.BackgroundServices;

public class ForecastOccurrenceReconciliationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ForecastOccurrenceReconciliationService> _logger;

    public ForecastOccurrenceReconciliationService(
        IServiceProvider serviceProvider,
        ILogger<ForecastOccurrenceReconciliationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ReconcileAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ReconcileAsync(stoppingToken);
        }
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<SynchronizeForecastOccurrencesCommandHandler>();
            await handler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling forecast occurrences");
        }
    }
}
