namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

public class CreateIncomeCommand
{
    public string Description { get; set; } = string.Empty;
    public Guid? ForecastOccurrenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? IdempotencyKey { get; set; }

    public CreateIncomeCommand() { }

    public CreateIncomeCommand(
        string description,
        decimal amount,
        DateTime date,
        string? idempotencyKey = null,
        Guid? forecastOccurrenceId = null)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Amount = amount;
        Date = date;
        IdempotencyKey = idempotencyKey;
        ForecastOccurrenceId = forecastOccurrenceId;
    }
}
