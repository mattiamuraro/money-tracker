namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

public class UpdateIncomeCommand
{
    public Guid IncomeId { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public Guid ModifiedById { get; set; }

    public UpdateIncomeCommand() { }

    public UpdateIncomeCommand(
        Guid incomeId,
        Guid modifiedById,
        string? description = null,
        decimal? amount = null,
        DateTime? date = null)
    {
        IncomeId = incomeId;
        ModifiedById = modifiedById;
        Description = description;
        Amount = amount;
        Date = date;
    }
}
