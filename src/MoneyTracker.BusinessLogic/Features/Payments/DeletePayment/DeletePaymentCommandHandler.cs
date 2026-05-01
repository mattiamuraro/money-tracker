using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;

/// <summary>
/// Handler for DeletePaymentCommand. Implements soft delete.
/// </summary>
public class DeletePaymentCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DeletePaymentCommand>
{
    public async Task Handle(DeletePaymentCommand command, CancellationToken cancellationToken)
    {
        if (!command.OccurrenceAction.TryParseOccurrenceAction(out var parsedAction))
            throw new BadRequestException("Occurrence action must be Auto, Reopen, or Skip.");

        var payment = await dbContext.Payments.FindAsync(
            new object[] { command.PaymentId }, cancellationToken: cancellationToken);
        if (payment == null)
            throw new EntityNotFoundException($"Payment with id {command.PaymentId} not found");

        if (payment.ForecastOccurrenceId.HasValue)
        {
            var occurrence = await dbContext.ForecastOccurrences
                .FirstOrDefaultAsync(x => x.Id == payment.ForecastOccurrenceId.Value, cancellationToken);
            if (occurrence != null)
                ApplyOccurrenceAction(occurrence, parsedAction);
        }

        payment.IsDeleted = true;
        dbContext.Payments.Update(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyOccurrenceAction(ForecastOccurrence occurrence, ForecastOccurrenceDeleteAction action)
    {
        occurrence.ForecastOccurrenceStatusId = ResolveOccurrenceStatusId(occurrence.ExpectedDate, action);
        occurrence.ValidatedAt = null;
    }

    private static Guid ResolveOccurrenceStatusId(DateOnly expectedDate, ForecastOccurrenceDeleteAction action) =>
        action switch
        {
            ForecastOccurrenceDeleteAction.Reopen => ForecastOccurrenceStatus.PendingId,
            ForecastOccurrenceDeleteAction.Skip => ForecastOccurrenceStatus.SkippedId,
            _ => expectedDate >= DateOnly.FromDateTime(DateTime.Today)
                ? ForecastOccurrenceStatus.PendingId
                : ForecastOccurrenceStatus.SkippedId
        };
}
