using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommand
{
    public Guid IncomeId { get; set; }
    public Guid DeletedBy { get; set; }
    public ForecastOccurrenceDeleteAction OccurrenceAction { get; set; } = ForecastOccurrenceDeleteAction.Auto;

    public DeleteIncomeCommand() { }

    public DeleteIncomeCommand(Guid incomeId, Guid deletedBy, ForecastOccurrenceDeleteAction occurrenceAction = ForecastOccurrenceDeleteAction.Auto)
    {
        IncomeId = incomeId;
        DeletedBy = deletedBy;
        OccurrenceAction = occurrenceAction;
    }
}
