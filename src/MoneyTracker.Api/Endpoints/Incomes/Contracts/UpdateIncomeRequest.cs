namespace MoneyTracker.Api.Endpoints.Incomes.Contracts;

public class UpdateIncomeRequest
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
}
