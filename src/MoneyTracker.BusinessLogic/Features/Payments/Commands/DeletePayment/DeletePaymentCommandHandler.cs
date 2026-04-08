using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.DeletePayment;

/// <summary>
/// Handler per il command DeletePaymentCommand
/// Implements soft delete - marks payment as deleted without removing from database
/// </summary>
public class DeletePaymentCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DeletePaymentCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments.FindAsync(
            new object[] { request.PaymentId },
            cancellationToken: cancellationToken);

        if (payment == null)
            return false;

        // Soft delete: mark as deleted instead of removing
        payment.Delete(request.DeletedBy ?? "System");

        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
