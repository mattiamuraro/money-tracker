using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;

/// <summary>
/// Handler per il command UpdatePaymentCommand
/// </summary>
public class UpdatePaymentCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public UpdatePaymentCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments.FindAsync(
            new object[] { request.PaymentId },
            cancellationToken: cancellationToken);

        if (payment == null)
            return false;

        // Aggiorna solo i campi forniti
        if (!string.IsNullOrEmpty(request.Description))
            payment.Description = request.Description;

        if (request.PaymentCategoryId.HasValue)
        {
            var categoryExists = await _dbContext.PaymentCategories.AnyAsync(
                category => category.Id == request.PaymentCategoryId.Value,
                cancellationToken);

            if (!categoryExists)
                throw new InvalidOperationException($"PaymentCategory with id {request.PaymentCategoryId.Value} not found");

            payment.PaymentCategoryId = request.PaymentCategoryId.Value;
        }

        if (request.Amount.HasValue)
            payment.Amount = request.Amount.Value;

        if (request.Date.HasValue)
            payment.Date = request.Date.Value;

        if (request.IsOneShot.HasValue)
            payment.IsOneShot = request.IsOneShot.Value;

        payment.ModifiedAt = DateTime.UtcNow;
        payment.ModifiedBy = request.ModifiedBy;

        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
