using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    : IHandler<DeleteForecastDefinitionCommand>
{
    public async Task Handle(DeleteForecastDefinitionCommand request, CancellationToken cancellationToken)
    {
        Guid? forecastDefinitionId = null;

        var expense = await dbContext.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);
        if (expense != null)
        {
            expense.IsActive = false;
            forecastDefinitionId = expense.Id;
        }

        var income = await dbContext.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);
        if (income != null)
        {
            income.IsActive = false;
            forecastDefinitionId = income.Id;
        }

        if (!forecastDefinitionId.HasValue)
            throw new EntityNotFoundException($"No active forecast found with ID '{request.Id}'.");

        await DeleteOccurrencesAsync(forecastDefinitionId.Value, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteOccurrencesAsync(Guid id, CancellationToken cancellationToken)
    {
        var occurrences = await dbContext.ForecastOccurrences
            .Where(x => x.ForecastDefinitionId == id && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
            .ToListAsync(cancellationToken);
        foreach (var occurrence in occurrences)
            occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;
    }
}
