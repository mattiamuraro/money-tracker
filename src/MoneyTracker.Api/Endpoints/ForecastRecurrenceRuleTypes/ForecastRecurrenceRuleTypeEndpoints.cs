using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes.Contracts;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes
{
    public static class ForecastRecurrenceRuleTypeEndpoints
    {
        internal static WebApplication AddForecastRecurrenceRuleTypeApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/forecast-recurrence-rule-types")
                        .WithTags("Forecast Recurrence Rule Types")
                        .RequireAuthorization();

            group.MapGet("/", static async ([FromServices] GetForecastRecurrenceRuleTypesQueryHandler handler, CancellationToken cancellationToken) =>
                {
                    var types = await handler.Handle(new GetForecastRecurrenceRuleTypesQuery(), cancellationToken);
                    var response = types.Select(t => new ForecastRecurrenceRuleTypeResponse
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Code = t.Code
                    });
                    return Results.Ok(response);
                })
                .WithName("GetForecastRecurrenceRuleTypes")
                .WithDescription("Retrieves forecast recurrence rule types")
                .Produces<List<ForecastRecurrenceRuleTypeResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
