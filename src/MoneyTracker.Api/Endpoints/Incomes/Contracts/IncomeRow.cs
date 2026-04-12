namespace MoneyTracker.Api.Endpoints.Incomes.Contracts;

public class IncomeRow
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
