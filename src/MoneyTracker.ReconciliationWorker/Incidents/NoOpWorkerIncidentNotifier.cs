using Microsoft.Extensions.Logging;

namespace MoneyTracker.ReconciliationWorker.Incidents;

public class NoOpWorkerIncidentNotifier(ILogger<NoOpWorkerIncidentNotifier> logger) : IWorkerIncidentNotifier
{
    private static readonly EventId EscalationObservedEventId = new(3101, nameof(EscalationObservedEventId));

    public Task NotifyEscalationAsync(
        int consecutiveFailures,
        TimeSpan failureDuration,
        TimeSpan nextRetryDelay,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            EscalationObservedEventId,
            "Escalation observed by no-op notifier. ConsecutiveFailures={ConsecutiveFailures}, FailureDuration={FailureDuration}, NextRetryDelay={NextRetryDelay}.",
            consecutiveFailures,
            failureDuration,
            nextRetryDelay);

        return Task.CompletedTask;
    }
}
