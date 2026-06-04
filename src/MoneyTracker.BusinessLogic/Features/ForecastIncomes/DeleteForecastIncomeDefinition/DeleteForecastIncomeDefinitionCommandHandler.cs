using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;

public class DeleteForecastIncomeDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DeleteForecastIncomeDefinitionCommand>
{
    public async Task Handle(DeleteForecastIncomeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var income = await dbContext.ForecastIncomes
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (income is null)
            throw new EntityNotFoundException($"No active forecast income found with ID '{request.Id}'.");

        income.IsActive = false;

        var occurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == request.Id
                && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
            .ToListAsync(cancellationToken);

        foreach (var occurrence in occurrences)
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
