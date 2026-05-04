using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;

public class GetForecastIncomeDefinitionsQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastIncomeDefinitionsQuery, List<ForecastIncomeDefinitionRow>>
{
    public async Task<List<ForecastIncomeDefinitionRow>> Handle(GetForecastIncomeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.ForecastIncomes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RecurrenceStart)
            .ThenBy(x => x.Description)
            .Select(x => new ForecastIncomeDefinitionRow
            {
                Id = x.Id,
                ForecastRecurrenceRuleTypeId = x.ForecastRecurrenceRuleTypeId,
                Description = x.Description,
                Amount = x.Amount,
                RecurrenceStart = x.RecurrenceStart,
                RecurrenceEnd = x.RecurrenceEnd,
                Interval = x.Interval ?? 1
            })
            .ToListAsync(cancellationToken);
    }
}
