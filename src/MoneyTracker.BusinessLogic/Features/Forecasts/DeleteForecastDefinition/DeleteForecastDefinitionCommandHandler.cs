using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Services;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;
    private readonly ForecastOccurrencesService _forecastOccurrencesService;

    public DeleteForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
        _forecastOccurrencesService = new ForecastOccurrencesService(dbContext);
    }

    public async Task<bool> Handle(DeleteForecastDefinitionCommand request, CancellationToken cancellationToken)
    {
        var expense = await _dbContext.ForecastExpenses
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense != null)
        {
            expense.IsActive = false;
            await _forecastOccurrencesService.DeleteOccurencesAsync(expense.Id, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        var income = await _dbContext.ForecastIncomes
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (income != null)
        {
            income.IsActive = false;
            await _forecastOccurrencesService.SynchronizeAsync(income, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        throw new EntityNotFoundException($"No active forecast found with ID '{request.Id}'.");


    }
}
