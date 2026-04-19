using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

namespace MoneyTracker.Api.Services
{
    public class ForecastRecurrenceRuleTypeService
    {
        private readonly GetForecastRecurrenceRuleTypesQueryHandler _getForecastRecurrenceRuleTypesQueryHandler;
        private readonly ILogger<ForecastRecurrenceRuleTypeService> _logger;

        public ForecastRecurrenceRuleTypeService(
            GetForecastRecurrenceRuleTypesQueryHandler getForecastRecurrenceRuleTypesQueryHandler,
            ILogger<ForecastRecurrenceRuleTypeService> logger)
        {
            _getForecastRecurrenceRuleTypesQueryHandler = getForecastRecurrenceRuleTypesQueryHandler;
            _logger = logger;
        }

        public async Task<IResult> GetForecastRecurrenceRuleTypesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetForecastRecurrenceRuleTypesQuery();
                var types = await _getForecastRecurrenceRuleTypesQueryHandler.Handle(query, cancellationToken);

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
