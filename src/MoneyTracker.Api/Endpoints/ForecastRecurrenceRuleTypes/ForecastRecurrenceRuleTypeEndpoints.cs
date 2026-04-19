using Microsoft.AspNetCore.Mvc;
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
                    try
                    {
                        var types = await handler.Handle(new GetForecastRecurrenceRuleTypesQuery(), cancellationToken);
                        return Results.Ok(types);
                    }
                    catch (Exception)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Error retrieving forecast recurrence rule types");
                    }
                })
                .WithName("GetForecastRecurrenceRuleTypes")
                .WithDescription("Retrieves forecast recurrence rule types")
                .Produces<List<ForecastRecurrenceRuleTypeDto>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
