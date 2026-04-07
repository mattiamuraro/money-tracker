using MediatR;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;

/// <summary>
/// Command per creare un nuovo pagamento
/// </summary>
public class CreatePaymentCommand : IRequest<Guid>
{
    public string Description { get; set; } = string.Empty;
    public Guid PaymentCategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsOneShot { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

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
        string createdBy,
        bool isOneShot = false,
        string? idempotencyKey = null)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        PaymentCategoryId = paymentCategoryId;
        Amount = amount;
        Date = date;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        IsOneShot = isOneShot;
        IdempotencyKey = idempotencyKey;
    }
}
