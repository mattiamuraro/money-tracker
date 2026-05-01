using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;

/// <summary>
/// Handler for the CreatePaymentCommand
/// </summary>
public class CreatePaymentCommandHandler(
    IValidator<CreatePaymentCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey, cancellationToken);
            if (existing != null)
                return existing.Id;
        }

        var occurrence = request.ForecastOccurrenceId.HasValue
            ? await dbContext.ForecastOccurrences.FirstOrDefaultAsync(
                x => x.Id == request.ForecastOccurrenceId.Value && !x.IsIncome,
                cancellationToken)
            : null;

        if (request.ForecastOccurrenceId.HasValue)
        {
            if (occurrence == null)
                throw new InvalidOperationException($"ForecastOccurrence with id {request.ForecastOccurrenceId.Value} not found");
            if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
                throw new InvalidOperationException($"ForecastOccurrence with id {request.ForecastOccurrenceId.Value} is not pending");
        }

        var category = await dbContext.PaymentCategories.FindAsync(
            new object[] { request.PaymentCategoryId }, cancellationToken: cancellationToken);
        if (category == null)
            throw new InvalidOperationException($"PaymentCategory with id {request.PaymentCategoryId} not found");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            PaymentCategoryId = request.PaymentCategoryId,
            ForecastOccurrenceId = request.ForecastOccurrenceId,
            Amount = request.Amount,
            Date = request.Date,
            IsOneShot = request.IsOneShot,
            IdempotencyKey = string.IsNullOrEmpty(request.IdempotencyKey) ? null : request.IdempotencyKey
        };

        if (occurrence != null)
        {
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId;
            occurrence.ValidatedAt = DateTime.UtcNow;
        }

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
