using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

public class GetForecastRecurrenceRuleTypesQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetForecastRecurrenceRuleTypesQuery, List<ForecastRecurrenceRuleTypeDto>>
{
    public async Task<List<ForecastRecurrenceRuleTypeDto>> Handle(GetForecastRecurrenceRuleTypesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.ForecastRecurrenceRuleTypes
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
