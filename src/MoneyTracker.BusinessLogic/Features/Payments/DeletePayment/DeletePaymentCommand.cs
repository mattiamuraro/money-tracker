using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;

/// <summary>
/// Command per soft-delete un pagamento
/// </summary>
public class DeletePaymentCommand
{
    public Guid PaymentId { get; set; }
    public string? OccurrenceAction { get; set; }

    public DeletePaymentCommand() { }

    public DeletePaymentCommand(Guid paymentId, string? occurrenceAction)
    {
        PaymentId = paymentId;
        OccurrenceAction = occurrenceAction;
    }
}
