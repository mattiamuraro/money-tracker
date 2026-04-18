using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.Api.Services
{
    public class ForecastRecurrenceRuleTypeService
    {
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly ILogger<ForecastRecurrenceRuleTypeService> _logger;

        public ForecastRecurrenceRuleTypeService(MoneyTrackerDbContext dbContext, ILogger<ForecastRecurrenceRuleTypeService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IResult> GetForecastRecurrenceRuleTypesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var types = await _dbContext.ForecastRecurrenceRuleTypes
                    .AsNoTracking()
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new ForecastRecurrenceRuleTypeDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast recurrence rule types");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast recurrence rule types");
            }
        }
    }
}
