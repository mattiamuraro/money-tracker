namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;

public class DiscardForecastExpenseOccurrenceCommand
{
    public Guid Id { get; set; }

    public DiscardForecastExpenseOccurrenceCommand(Guid id)
    {
        Id = id;
    }
}
