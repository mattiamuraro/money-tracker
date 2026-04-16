using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.DeletePayment;

/// <summary>
/// Command per soft-delete un pagamento
/// </summary>
public class DeletePaymentCommand
{
    public Guid PaymentId { get; set; }
    public Guid DeletedBy { get; set; }
    public ForecastOccurrenceDeleteAction OccurrenceAction { get; set; } = ForecastOccurrenceDeleteAction.Auto;

    public DeletePaymentCommand() { }

    public DeletePaymentCommand(Guid paymentId, Guid deletedBy, ForecastOccurrenceDeleteAction occurrenceAction = ForecastOccurrenceDeleteAction.Auto)
    {
        PaymentId = paymentId;
        DeletedBy = deletedBy;
        OccurrenceAction = occurrenceAction;
    }
}
