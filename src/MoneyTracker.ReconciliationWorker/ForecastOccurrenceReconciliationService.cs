using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.ReconciliationWorker.Incidents;
using MoneyTracker.ReconciliationWorker.Options;

namespace MoneyTracker.ReconciliationWorker;

public class ForecastOccurrenceReconciliationService : BackgroundService
{
    private static readonly EventId ReconciliationStartedEventId = new(3001, nameof(ReconciliationStartedEventId));
    private static readonly EventId ReconciliationSucceededEventId = new(3002, nameof(ReconciliationSucceededEventId));
    private static readonly EventId ReconciliationCanceledEventId = new(3003, nameof(ReconciliationCanceledEventId));
    private static readonly EventId ReconciliationFailedEventId = new(3004, nameof(ReconciliationFailedEventId));
    private static readonly EventId RetryScheduledEventId = new(3005, nameof(RetryScheduledEventId));
    private static readonly EventId ServiceCanceledEventId = new(3006, nameof(ServiceCanceledEventId));
    private static readonly EventId EscalationTriggeredEventId = new(3007, nameof(EscalationTriggeredEventId));
    private static readonly EventId ServiceStoppingOnEscalationEventId = new(3008, nameof(ServiceStoppingOnEscalationEventId));
    private static readonly EventId EscalationNotificationFailedEventId = new(3009, nameof(EscalationNotificationFailedEventId));

    private static readonly Meter WorkerMeter = new("MoneyTracker.Workers");
    private static readonly Counter<long> ReconciliationRunsCounter = WorkerMeter.CreateCounter<long>("reconciliation_runs_total");
    private static readonly Counter<long> ReconciliationSucceededCounter = WorkerMeter.CreateCounter<long>("reconciliation_succeeded_total");
    private static readonly Counter<long> ReconciliationFailedCounter = WorkerMeter.CreateCounter<long>("reconciliation_failed_total");
    private static readonly Counter<long> ReconciliationCanceledCounter = WorkerMeter.CreateCounter<long>("reconciliation_canceled_total");
    private static readonly Counter<long> ReconciliationRetryScheduledCounter = WorkerMeter.CreateCounter<long>("reconciliation_retry_scheduled_total");
    private static readonly Counter<long> ReconciliationEscalationsCounter = WorkerMeter.CreateCounter<long>("reconciliation_escalations_total");

    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);
    private static readonly TimeSpan MaxFailureDelay = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan AlertThresholdDelay = TimeSpan.FromMinutes(15);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ForecastOccurrenceReconciliationService> _logger;
    private readonly WorkerResilienceOptions _resilienceOptions;
    private readonly IWorkerIncidentNotifier _incidentNotifier;

    public ForecastOccurrenceReconciliationService(
        IServiceProvider serviceProvider,
        ILogger<ForecastOccurrenceReconciliationService> logger,
        IOptions<WorkerResilienceOptions> resilienceOptions,
        IWorkerIncidentNotifier incidentNotifier)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(resilienceOptions);
        ArgumentNullException.ThrowIfNull(incidentNotifier);

        _serviceProvider = serviceProvider;
        _logger = logger;
        _incidentNotifier = incidentNotifier;
        _resilienceOptions = resilienceOptions.Value;

        if (_resilienceOptions.MaxConsecutiveFailures < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(resilienceOptions), "MaxConsecutiveFailures must be at least 1.");
        }

        if (_resilienceOptions.EscalationWindowMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(resilienceOptions), "EscalationWindowMinutes must be at least 1.");
        }

        if (_resilienceOptions.EscalationCooldownMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(resilienceOptions), "EscalationCooldownMinutes must be at least 1.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consecutiveFailures = 0;
        DateTimeOffset? firstFailureAtUtc = null;
        DateTimeOffset? lastEscalationAtUtc = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var runId = Guid.NewGuid().ToString("N");
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["RunId"] = runId
            });

            var succeeded = await ReconcileAsync(stoppingToken);

            if (succeeded)
            {
                consecutiveFailures = 0;
                firstFailureAtUtc = null;
            }
            else
            {
                consecutiveFailures++;
                firstFailureAtUtc ??= DateTimeOffset.UtcNow;
            }

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

            if (!succeeded)
            {
                await HandleEscalationAsync(
                    consecutiveFailures,
                    firstFailureAtUtc,
                    delay,
                    lastEscalationAtUtc,
                    stoppingToken);

                var nowUtc = DateTimeOffset.UtcNow;
                if (ShouldEscalate(nowUtc, consecutiveFailures, firstFailureAtUtc)
                    && !IsEscalationCooldownActive(nowUtc, lastEscalationAtUtc))
                {
                    lastEscalationAtUtc = nowUtc;
                }
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

    private async Task HandleEscalationAsync(
        int consecutiveFailures,
        DateTimeOffset? firstFailureAtUtc,
        TimeSpan nextRetryDelay,
        DateTimeOffset? lastEscalationAtUtc,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        if (!ShouldEscalate(nowUtc, consecutiveFailures, firstFailureAtUtc)
            || IsEscalationCooldownActive(nowUtc, lastEscalationAtUtc))
        {
            return;
        }

        var failureDuration = firstFailureAtUtc.HasValue
            ? nowUtc - firstFailureAtUtc.Value
            : TimeSpan.Zero;

        ReconciliationEscalationsCounter.Add(1);

        _logger.LogError(
            EscalationTriggeredEventId,
            "Reconciliation escalation triggered after {ConsecutiveFailures} consecutive failures over {FailureDuration}. Next retry in {NextRetryDelay}. StopServiceOnEscalation={StopServiceOnEscalation}.",
            consecutiveFailures,
            failureDuration,
            nextRetryDelay,
            _resilienceOptions.StopServiceOnEscalation);

        await TryNotifyEscalationAsync(consecutiveFailures, failureDuration, nextRetryDelay, cancellationToken);

        if (_resilienceOptions.StopServiceOnEscalation)
        {
            _logger.LogCritical(
                ServiceStoppingOnEscalationEventId,
                "Stopping reconciliation service because escalation policy requires fail-fast behavior.");

            throw new InvalidOperationException("Forecast occurrence reconciliation exceeded escalation thresholds.");
        }
    }

    private async Task TryNotifyEscalationAsync(
        int consecutiveFailures,
        TimeSpan failureDuration,
        TimeSpan nextRetryDelay,
        CancellationToken cancellationToken)
    {
        try
        {
            await _incidentNotifier.NotifyEscalationAsync(consecutiveFailures, failureDuration, nextRetryDelay, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EscalationNotificationFailedEventId,
                ex,
                "Failed to send reconciliation escalation notification.");
        }
    }

    private bool ShouldEscalate(DateTimeOffset nowUtc, int consecutiveFailures, DateTimeOffset? firstFailureAtUtc)
    {
        if (consecutiveFailures >= _resilienceOptions.MaxConsecutiveFailures)
        {
            return true;
        }

        if (!firstFailureAtUtc.HasValue)
        {
            return false;
        }

        var escalationWindow = TimeSpan.FromMinutes(_resilienceOptions.EscalationWindowMinutes);
        return nowUtc - firstFailureAtUtc.Value >= escalationWindow;
    }

    private bool IsEscalationCooldownActive(DateTimeOffset nowUtc, DateTimeOffset? lastEscalationAtUtc)
    {
        if (!lastEscalationAtUtc.HasValue)
        {
            return false;
        }

        var cooldown = TimeSpan.FromMinutes(_resilienceOptions.EscalationCooldownMinutes);
        return nowUtc - lastEscalationAtUtc.Value < cooldown;
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

