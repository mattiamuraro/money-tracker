using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes.Contracts;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;

namespace MoneyTracker.Api.Endpoints.ForecastRecurrenceRuleTypes
{
    public static class ForecastRecurrenceRuleTypeEndpoints
    {
        internal static WebApplication AddForecastRecurrenceRuleTypeApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/forecast-recurrence-rule-types")
                        .WithTags("Forecast Recurrence Rule Types")
                        .RequireAuthorization();

            group.MapGet("/", static async ([FromServices] IHandler<GetForecastRecurrenceRuleTypesQuery, List<ForecastRecurrenceRuleTypeDto>> handler, CancellationToken cancellationToken) =>
                {
                    var types = await handler.Handle(new GetForecastRecurrenceRuleTypesQuery(), cancellationToken);
                    var response = types.Select(static type => new ForecastRecurrenceRuleTypeResponse
                    {
                        Id = type.Id,
                        Name = type.Name,
                        Code = type.Code
                    });

                    return Results.Ok(response);
                })
                .WithName("GetForecastRecurrenceRuleTypes")
                .WithDescription("Retrieves forecast recurrence rule types")
                .Produces<List<ForecastRecurrenceRuleTypeResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
