using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;

public class DiscardPendingForecastOccurrenceCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DiscardPendingForecastOccurrenceCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DiscardPendingForecastOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.ForecastOccurrences
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (occurrence is null)
            throw new EntityNotFoundException($"Forecast Occurrence with id {request.Id} not found");

        if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
            throw new InvalidOperationException("Only pending forecast occurrences can be discarded.");

        occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId;
        occurrence.ValidatedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
