namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

public class CreatePaymentRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid PaymentCategoryId { get; set; }
    public Guid? ForecastOccurrenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsOneShot { get; set; }
    public string? IdempotencyKey { get; set; }
}
