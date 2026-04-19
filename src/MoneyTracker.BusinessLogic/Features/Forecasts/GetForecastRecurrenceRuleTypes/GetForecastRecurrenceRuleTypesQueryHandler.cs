using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

public class GetForecastRecurrenceRuleTypesQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastRecurrenceRuleTypesQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastRecurrenceRuleTypeDto>> Handle(GetForecastRecurrenceRuleTypesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ForecastRecurrenceRuleTypes
            .AsNoTracking()
            .OrderBy(x => x.OrderIndex)
            .Select(x => new ForecastRecurrenceRuleTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync(cancellationToken);
    }
}
