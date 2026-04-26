using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DeleteForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteForecastDefinitionCommand request, CancellationToken cancellationToken)
    {

        Guid? forecastDefinitionId = null;

        var expense = await _dbContext.ForecastExpenses.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense != null)
        {
            expense.IsActive = false;
            forecastDefinitionId = expense.Id;
        }

        var income = await _dbContext.ForecastIncomes.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (income != null)
        {
            income.IsActive = false;
            forecastDefinitionId = income.Id;
        }

        if (!forecastDefinitionId.HasValue)
            throw new EntityNotFoundException($"No active forecast found with ID '{request.Id}'.");

        await DeleteOccurencesAsync(forecastDefinitionId.Value, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteOccurencesAsync(Guid id, CancellationToken cancellationToken)
    {
        await _dbContext.ForecastOccurrences.Where(x => x.ForecastDefinitionId == id && x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                                            .ExecuteUpdateAsync(e => e.SetProperty(p => p.ForecastOccurrenceStatusId, ForecastOccurrenceStatus.CancelledId)
                                            , cancellationToken);
    }
}
