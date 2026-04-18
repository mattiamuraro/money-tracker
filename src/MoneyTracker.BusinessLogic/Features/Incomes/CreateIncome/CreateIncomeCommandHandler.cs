using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

public class CreateIncomeCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateIncomeCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _dbContext.Incomes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (existing is not null)
                return existing.Id;
        }

        var occurrence = request.ForecastOccurrenceId.HasValue
            ? await _dbContext.ForecastOccurrences.FirstOrDefaultAsync(
                x => x.Id == request.ForecastOccurrenceId.Value && x.IsIncome,
                cancellationToken)
            : null;

        if (request.ForecastOccurrenceId.HasValue)
        {
            if (occurrence == null)
                throw new InvalidOperationException($"ForecastOccurrence with id {request.ForecastOccurrenceId.Value} not found");

            if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
                throw new InvalidOperationException($"ForecastOccurrence with id {request.ForecastOccurrenceId.Value} is not pending");
        }

        var income = new Income
        {
            Id = Guid.NewGuid(),
            Description = request.Description.Trim(),
            ForecastOccurrenceId = request.ForecastOccurrenceId,
            Amount = request.Amount,
            Date = request.Date,
            IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey,
            CreatedById = request.CreatedById,
            ModifiedById = request.CreatedById,
        };

        if (occurrence != null)
        {
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.ConfirmedId;
            occurrence.ValidatedAt = DateTime.UtcNow;
        }

        _dbContext.Incomes.Add(income);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return income.Id;
    }
}
