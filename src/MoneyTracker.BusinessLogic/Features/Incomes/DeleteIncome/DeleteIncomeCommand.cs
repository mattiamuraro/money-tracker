namespace MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;

public class DeleteIncomeCommand
{
    public Guid IncomeId { get; set; }
    public Guid DeletedBy { get; set; }
    public string? OccurrenceAction { get; set; }

    public DeleteIncomeCommand() { }

    public DeleteIncomeCommand(Guid incomeId, string? occurrenceAction)
    {
        IncomeId = incomeId;
        OccurrenceAction = occurrenceAction;
    }
}
