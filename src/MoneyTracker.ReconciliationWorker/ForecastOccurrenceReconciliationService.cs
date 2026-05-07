using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.ReconciliationWorker;

public class ForecastOccurrenceReconciliationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);
    private static readonly TimeSpan MaxFailureDelay = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan AlertThresholdDelay = TimeSpan.FromMinutes(15);

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
        var consecutiveFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var succeeded = await ReconcileAsync(stoppingToken);
            consecutiveFailures = succeeded ? 0 : consecutiveFailures + 1;

            var delay = succeeded
                ? Interval
                : CalculateFailureDelay(consecutiveFailures);

            if (!succeeded && delay >= AlertThresholdDelay)
            {
                _logger.LogWarning(
                    "Forecast occurrence reconciliation has failed {ConsecutiveFailures} consecutive times. Next retry in {Delay}.",
                    consecutiveFailures,
                    delay);
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Forecast occurrence reconciliation has been canceled.");
                break;
            }
        }
    }

    private async Task<bool> ReconcileAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IHandler<SynchronizeForecastOccurrencesCommand>>();
            await handler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Forecast occurrence reconciliation canceled during execution.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling forecast occurrences");
            return false;
        }
    }

    private static TimeSpan CalculateFailureDelay(int consecutiveFailures)
    {
        var exponent = Math.Clamp(consecutiveFailures - 1, 0, 5);
        var minutes = 1 << exponent;
        var baseDelay = TimeSpan.FromMinutes(minutes);
        var cappedDelay = baseDelay <= MaxFailureDelay ? baseDelay : MaxFailureDelay;
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1_000));
        return cappedDelay + jitter;
    }
}

