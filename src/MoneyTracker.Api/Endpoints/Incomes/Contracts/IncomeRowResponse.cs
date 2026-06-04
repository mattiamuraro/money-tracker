namespace MoneyTracker.Api.Endpoints.Incomes.Contracts
{
    public class IncomeRowResponse
    {
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public Guid? ForecastOccurrenceId { get; set; }
        public DateOnly? ForecastExpectedDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
