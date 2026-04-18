namespace MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;

/// <summary>
/// Command per creare un nuovo pagamento
/// </summary>
public class CreatePaymentCommand
{
    public string Description { get; set; } = string.Empty;
    public Guid PaymentCategoryId { get; set; }
    public Guid? ForecastOccurrenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsOneShot { get; set; }
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate payments
    /// </summary>
    public string? IdempotencyKey { get; set; }

    public CreatePaymentCommand() { }

    public CreatePaymentCommand(
        string description,
        Guid paymentCategoryId,
        decimal amount,
        DateTime date,
        Guid createdById,
        bool isOneShot = false,
        string? idempotencyKey = null,
        Guid? forecastOccurrenceId = null)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        PaymentCategoryId = paymentCategoryId;
        Amount = amount;
        Date = date;
        CreatedById = createdById;
        IsOneShot = isOneShot;
        IdempotencyKey = idempotencyKey;
        ForecastOccurrenceId = forecastOccurrenceId;
    }
}
