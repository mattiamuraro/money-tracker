using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;

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

    public async Task Handle(DeletePaymentCommand command, CancellationToken cancellationToken)
    {

        if (!command.OccurrenceAction.TryParseOccurrenceAction(out var parsedAction))
            throw new BadRequestException("Occurrence action must be Auto, Reopen, or Skip.");


        var payment = await _dbContext.Payments.FindAsync(
            new object[] { command.PaymentId },
            cancellationToken: cancellationToken);

        if (payment == null)
            throw new EntityNotFoundException($"Payment with id {command.PaymentId} not found");

        if (payment.ForecastOccurrenceId.HasValue)
        {
            var occurrence = await _dbContext.ForecastOccurrences
                .FirstOrDefaultAsync(x => x.Id == payment.ForecastOccurrenceId.Value, cancellationToken);

            if (occurrence != null)
            {
                ApplyOccurrenceAction(occurrence, parsedAction);
            }
        }

        payment.IsDeleted = true;

        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
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
