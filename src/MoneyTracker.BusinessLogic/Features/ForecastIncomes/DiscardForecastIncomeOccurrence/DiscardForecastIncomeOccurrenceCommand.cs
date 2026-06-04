namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;

public class DiscardForecastIncomeOccurrenceCommand
{
    public Guid Id { get; set; }

    public DiscardForecastIncomeOccurrenceCommand(Guid id)
    {
        Id = id;
    }
}
