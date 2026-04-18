namespace MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;

public class DiscardPendingForecastOccurrenceCommand
{
    public Guid Id { get; set; }

    public DiscardPendingForecastOccurrenceCommand(Guid id)
    {
        Id = id;
    }
}
