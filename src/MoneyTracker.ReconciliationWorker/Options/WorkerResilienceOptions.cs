namespace MoneyTracker.ReconciliationWorker.Options;

public sealed class WorkerResilienceOptions
{
    public const string SectionName = "Worker:Resilience";

    public int MaxConsecutiveFailures { get; set; } = 5;

    public int EscalationWindowMinutes { get; set; } = 30;

    public bool StopServiceOnEscalation { get; set; }

    public int EscalationCooldownMinutes { get; set; } = 15;
}
