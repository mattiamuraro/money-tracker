using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;

/// <summary>
/// Handler per il command CreatePaymentCommand
/// </summary>
public class CreatePaymentCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public CreatePaymentCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // If an idempotency key is present, return the existing payment ID without creating a duplicate
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (existing != null)
                return existing.Id;
        }

        var occurrence = request.ForecastOccurrenceId.HasValue
            ? await _dbContext.ForecastOccurrences.FirstOrDefaultAsync(
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

        // Verificare che la categoria esista
        var category = await _dbContext.PaymentCategories.FindAsync(
            new object[] { request.PaymentCategoryId },
            cancellationToken: cancellationToken);

        if (category == null)
            throw new InvalidOperationException($"PaymentCategory with id {request.PaymentCategoryId} not found");

        var payment = new Data.Payment
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            PaymentCategoryId = request.PaymentCategoryId,
            ForecastOccurrenceId = request.ForecastOccurrenceId,
            Amount = request.Amount,
            Date = request.Date,
            IsOneShot = request.IsOneShot,
            IdempotencyKey = string.IsNullOrEmpty(request.IdempotencyKey) ? null : request.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            CreatedById = request.CreatedById,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = request.CreatedById
        };

        if (occurrence != null)
        {
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId;
            occurrence.ValidatedAt = DateTime.UtcNow;
        }

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
