using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.ReconciliationWorker;

public class ForecastOccurrenceReconciliationService : BackgroundService
{
    private static readonly EventId ReconciliationStartedEventId = new(3001, nameof(ReconciliationStartedEventId));
    private static readonly EventId ReconciliationSucceededEventId = new(3002, nameof(ReconciliationSucceededEventId));
    private static readonly EventId ReconciliationCanceledEventId = new(3003, nameof(ReconciliationCanceledEventId));
    private static readonly EventId ReconciliationFailedEventId = new(3004, nameof(ReconciliationFailedEventId));
    private static readonly EventId RetryScheduledEventId = new(3005, nameof(RetryScheduledEventId));
    private static readonly EventId ServiceCanceledEventId = new(3006, nameof(ServiceCanceledEventId));

    private static readonly Meter WorkerMeter = new("MoneyTracker.Workers");
    private static readonly Counter<long> ReconciliationRunsCounter = WorkerMeter.CreateCounter<long>("reconciliation_runs_total");
    private static readonly Counter<long> ReconciliationSucceededCounter = WorkerMeter.CreateCounter<long>("reconciliation_succeeded_total");
    private static readonly Counter<long> ReconciliationFailedCounter = WorkerMeter.CreateCounter<long>("reconciliation_failed_total");
    private static readonly Counter<long> ReconciliationCanceledCounter = WorkerMeter.CreateCounter<long>("reconciliation_canceled_total");
    private static readonly Counter<long> ReconciliationRetryScheduledCounter = WorkerMeter.CreateCounter<long>("reconciliation_retry_scheduled_total");

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
            var runId = Guid.NewGuid().ToString("N");
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["RunId"] = runId
            });

            var succeeded = await ReconcileAsync(stoppingToken);
            consecutiveFailures = succeeded ? 0 : consecutiveFailures + 1;

            var delay = succeeded
                ? Interval
                : CalculateFailureDelay(consecutiveFailures);

            if (!succeeded && delay >= AlertThresholdDelay)
            {
                ReconciliationRetryScheduledCounter.Add(1);
                _logger.LogWarning(
                    RetryScheduledEventId,
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
                _logger.LogInformation(ServiceCanceledEventId, "Forecast occurrence reconciliation has been canceled.");
                break;
            }
        }
    }

    private async Task<bool> ReconcileAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        ReconciliationRunsCounter.Add(1);

        try
        {
            _logger.LogInformation(ReconciliationStartedEventId, "Forecast occurrence reconciliation started.");
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IHandler<SynchronizeForecastOccurrencesCommand>>();
            await handler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
            stopwatch.Stop();
            ReconciliationSucceededCounter.Add(1);
            _logger.LogInformation(
                ReconciliationSucceededEventId,
                "Forecast occurrence reconciliation succeeded in {ElapsedMs}ms.",
                stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            ReconciliationCanceledCounter.Add(1);
            _logger.LogInformation(
                ReconciliationCanceledEventId,
                "Forecast occurrence reconciliation canceled during execution after {ElapsedMs}ms.",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ReconciliationFailedCounter.Add(1);
            _logger.LogError(
                ReconciliationFailedEventId,
                ex,
                "Error reconciling forecast occurrences after {ElapsedMs}ms.",
                stopwatch.ElapsedMilliseconds);
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

