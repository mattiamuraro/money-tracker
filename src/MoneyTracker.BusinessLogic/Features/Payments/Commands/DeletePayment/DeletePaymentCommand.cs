namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.DeletePayment;

/// <summary>
/// Command per soft-delete un pagamento
/// </summary>
public class DeletePaymentCommand
{
    public Guid PaymentId { get; set; }
    public string? DeletedBy { get; set; }

    public DeletePaymentCommand() { }

    public DeletePaymentCommand(Guid paymentId, string? deletedBy = null)
    {
        PaymentId = paymentId;
        DeletedBy = deletedBy;
    }
}
