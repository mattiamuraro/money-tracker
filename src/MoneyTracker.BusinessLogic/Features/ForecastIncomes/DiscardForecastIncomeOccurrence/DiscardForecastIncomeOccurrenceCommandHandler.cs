using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;

public class DiscardForecastIncomeOccurrenceCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DiscardForecastIncomeOccurrenceCommand>
{
    public async Task Handle(DiscardForecastIncomeOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var occurrence = await dbContext.ForecastOccurrences
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsIncome, cancellationToken);

        if (occurrence is null)
            throw new EntityNotFoundException($"Forecast income occurrence with id {request.Id} not found.");

        if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
            throw new ConflictException("Only pending forecast occurrences can be discarded.");

        occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId;
        occurrence.ValidatedAt = null;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
