using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;
using MoneyTracker.Api.Endpoints.ForecastIncomes.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;

namespace MoneyTracker.Api.Endpoints.ForecastIncomes;

public static class ForecastIncomeEndpoints
{
    internal static WebApplication AddForecastIncomeApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/forecast-incomes")
            .WithTags("Forecast Incomes")
            .RequireAuthorization();

        group.MapGet("/", static async (
                [FromServices] GetForecastIncomeRowsQueryHandler getForecastRowsHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                [FromQuery] DateOnly? startDate,
                [FromQuery] DateOnly? endDate,
                CancellationToken cancellationToken) =>
            {
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                var query = CreateGetForecastIncomeRowsQuery(startDate, endDate);
                var forecasts = await getForecastRowsHandler.Handle(query, cancellationToken);
                var response = forecasts.Select(f => f.ToForecastIncomeRowResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomes")
            .WithDescription("Retrieves forecast incomes for a given date range")
            .Produces<List<ForecastIncomeRowResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/definitions", static async (
                [FromServices] GetForecastIncomeDefinitionsQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var definitions = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), cancellationToken);
                var response = definitions.Select(d => d.ToForecastIncomeDefinitionResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomeDefinitions")
            .WithDescription("Retrieves all forecast income definitions")
            .Produces<List<ForecastIncomeDefinitionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/definitions", static async (
                [FromServices] CreateForecastIncomeDefinitionCommandHandler createHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                CreateForecastIncomeRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCreateForecastIncomeDefinitionCommand();
                var id = await createHandler.Handle(command, cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.Created($"/api/v1/forecast-incomes/definitions/{id}", id);
            })
            .WithName("CreateForecastIncomeDefinition")
            .WithDescription("Creates a new forecast income definition")
            .Accepts<CreateForecastIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/definitions/{id:guid}", static async (
                [FromServices] UpdateForecastIncomeDefinitionCommandHandler updateHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                Guid id,
                UpdateForecastIncomeRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToUpdateForecastIncomeDefinitionCommand(id);
                await updateHandler.Handle(command, cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.NoContent();
            })
            .WithName("UpdateForecastIncomeDefinition")
            .WithDescription("Updates an existing forecast income definition")
            .Accepts<UpdateForecastIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/definitions/{id:guid}", static async (
                [FromServices] DeleteForecastIncomeDefinitionCommandHandler deleteHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await deleteHandler.Handle(new DeleteForecastIncomeDefinitionCommand(id), cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.NoContent();
            })
            .WithName("DeleteForecastIncomeDefinition")
            .WithDescription("Deletes a forecast income definition")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/occurrences", static async (
                [FromServices] GetPendingForecastIncomeOccurrencesQueryHandler handler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                [AsParameters] ForecastIncomeOccurencesQuery request,
                CancellationToken cancellationToken) =>
            {
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                var query = request.ToGetPendingForecastIncomeOccurrencesQuery();
                var items = await handler.Handle(query, cancellationToken);
                var response = items.Select(o => o.ToForecastIncomeOccurrenceResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomeOccurrences")
            .WithDescription("Retrieves pending forecast income occurrences for a month")
            .Produces<List<ForecastIncomeOccurrenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/occurrences/{id:guid}", static async (
                [FromServices] DiscardForecastIncomeOccurrenceCommandHandler handler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(new DiscardForecastIncomeOccurrenceCommand(id), cancellationToken);

                return Results.NoContent();
            })
            .WithName("DiscardForecastIncomeOccurrence")
            .WithDescription("Discards a pending forecast income occurrence")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static GetForecastIncomeRowsQuery CreateGetForecastIncomeRowsQuery(DateOnly? startDate, DateOnly? endDate)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);
        var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        return new GetForecastIncomeRowsQuery(start, end);
    }
}
