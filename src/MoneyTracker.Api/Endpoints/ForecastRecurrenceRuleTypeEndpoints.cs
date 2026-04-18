using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;

namespace MoneyTracker.Api.Endpoints
{
    public static class ForecastRecurrenceRuleTypeEndpoints
    {
        internal static WebApplication AddForecastRecurrenceRuleTypeApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/forecast-recurrence-rule-types")
                        .WithTags("Forecast Recurrence Rule Types")
                        .RequireAuthorization();

            group.MapGet("/", static async ([FromServices] ForecastRecurrenceRuleTypeService forecastRecurrenceRuleTypeService, CancellationToken cancellationToken) =>
                    await forecastRecurrenceRuleTypeService.GetForecastRecurrenceRuleTypesAsync(cancellationToken))
                .WithName("GetForecastRecurrenceRuleTypes")
                .WithDescription("Retrieves forecast recurrence rule types")
                .Produces<List<ForecastRecurrenceRuleTypeDto>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
