namespace MoneyTracker.ReconciliationWorker.Incidents;

public interface IWorkerIncidentNotifier
{
    Task NotifyEscalationAsync(
        int consecutiveFailures,
        TimeSpan failureDuration,
        TimeSpan nextRetryDelay,
        CancellationToken cancellationToken);
}
