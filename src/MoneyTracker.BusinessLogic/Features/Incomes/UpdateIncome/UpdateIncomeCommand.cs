namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommand
{
    public Guid IncomeId { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }

    public UpdateIncomeCommand() { }

    public UpdateIncomeCommand(
        Guid incomeId,
        string? description = null,
        decimal? amount = null,
        DateTime? date = null)
    {
        IncomeId = incomeId;
        Description = description;
        Amount = amount;
        Date = date;
    }
}
