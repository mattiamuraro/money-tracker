using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
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

        if (payment.ForecastOccurrenceId.HasValue)
        {
            var occurrence = await _dbContext.ForecastOccurrences
                .FirstOrDefaultAsync(x => x.Id == payment.ForecastOccurrenceId.Value, cancellationToken);

            if (occurrence != null)
            {
                ApplyOccurrenceAction(occurrence, request.OccurrenceAction);
            }
        }

        payment.Delete(request.DeletedBy == Guid.Empty ? SystemUsers.SystemUserId : request.DeletedBy);

        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ApplyOccurrenceAction(ForecastOccurrence occurrence, ForecastOccurrenceDeleteAction action)
    {
        occurrence.ForecastOccurrenceStatusId = ResolveOccurrenceStatusId(occurrence.ExpectedDate, action);
        occurrence.ValidatedAt = null;
    }

    private static Guid ResolveOccurrenceStatusId(DateOnly expectedDate, ForecastOccurrenceDeleteAction action)
    {
        return action switch
        {
            ForecastOccurrenceDeleteAction.Reopen => ForecastOccurrenceStatus.PendingId,
            ForecastOccurrenceDeleteAction.Skip => ForecastOccurrenceStatus.SkippedId,
            _ => expectedDate >= DateOnly.FromDateTime(DateTime.Today)
                ? ForecastOccurrenceStatus.PendingId
                : ForecastOccurrenceStatus.SkippedId
        };
    }
}
