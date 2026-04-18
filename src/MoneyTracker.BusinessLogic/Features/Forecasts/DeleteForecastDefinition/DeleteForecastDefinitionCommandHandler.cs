using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;

public class DeleteForecastDefinitionCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public DeleteForecastDefinitionCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteForecastDefinitionCommand request, CancellationToken cancellationToken)
    {
        var expense = await _dbContext.ForecastExpenses
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (expense != null)
        {
            expense.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        var income = await _dbContext.ForecastIncomes
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (income != null)
        {
            income.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
