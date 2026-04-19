namespace MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

public class PaymentRow
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public Guid PaymentCategoryId { get; set; }
    public required string Category { get; set; }
    public Guid? ForecastOccurrenceId { get; set; }
    public DateOnly? ForecastExpectedDate { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsOneShot { get; set; }
}
