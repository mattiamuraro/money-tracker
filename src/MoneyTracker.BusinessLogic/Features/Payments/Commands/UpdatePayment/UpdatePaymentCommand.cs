namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;

/// <summary>
/// Command per aggiornare un pagamento
/// </summary>
public class UpdatePaymentCommand
{
    public Guid PaymentId { get; set; }
    public string? Description { get; set; }
    public Guid? PaymentCategoryId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public bool? IsOneShot { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;

    public UpdatePaymentCommand() { }

    public UpdatePaymentCommand(
        Guid paymentId,
        string modifiedBy,
        string? description = null,
        Guid? paymentCategoryId = null,
        decimal? amount = null,
        DateTime? date = null,
        bool? isOneShot = null)
    {
        PaymentId = paymentId;
        ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        Description = description;
        PaymentCategoryId = paymentCategoryId;
        Amount = amount;
        Date = date;
        IsOneShot = isOneShot;
    }
}
