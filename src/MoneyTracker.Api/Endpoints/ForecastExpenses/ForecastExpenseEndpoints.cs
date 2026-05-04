using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;
using MoneyTracker.Api.Endpoints.ForecastExpenses.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.Api.Endpoints.ForecastExpenses;

public static class ForecastExpenseEndpoints
{
    internal static WebApplication AddForecastExpenseApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/forecast-expenses")
            .WithTags("Forecast Expenses")
            .RequireAuthorization();

        group.MapGet("/", static async (
                [FromServices] GetForecastExpenseRowsQueryHandler getForecastRowsHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                [FromQuery] DateOnly? startDate,
                [FromQuery] DateOnly? endDate,
                CancellationToken cancellationToken) =>
            {
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                var query = CreateGetForecastExpenseRowsQuery(startDate, endDate);
                var forecasts = await getForecastRowsHandler.Handle(query, cancellationToken);
                var response = forecasts.Select(f => f.ToForecastExpenseRowResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastExpenses")
            .WithDescription("Retrieves forecast expenses for a given date range")
            .Produces<List<ForecastExpenseRowResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/definitions", static async (
                [FromServices] GetForecastExpenseDefinitionsQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var definitions = await handler.Handle(new GetForecastExpenseDefinitionsQuery(), cancellationToken);
                var response = definitions.Select(d => d.ToForecastExpenseDefinitionResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastExpenseDefinitions")
            .WithDescription("Retrieves all forecast expense definitions")
            .Produces<List<ForecastExpenseDefinitionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/definitions", static async (
                [FromServices] CreateForecastExpenseDefinitionCommandHandler createHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                CreateForecastExpenseRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCreateForecastExpenseDefinitionCommand();
                var id = await createHandler.Handle(command, cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.Created($"/api/v1/forecast-expenses/definitions/{id}", id);
            })
            .WithName("CreateForecastExpenseDefinition")
            .WithDescription("Creates a new forecast expense definition")
            .Accepts<CreateForecastExpenseRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/definitions/{id:guid}", static async (
                [FromServices] UpdateForecastExpenseDefinitionCommandHandler updateHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                Guid id,
                UpdateForecastExpenseRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToUpdateForecastExpenseDefinitionCommand(id);
                await updateHandler.Handle(command, cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.NoContent();
            })
            .WithName("UpdateForecastExpenseDefinition")
            .WithDescription("Updates an existing forecast expense definition")
            .Accepts<UpdateForecastExpenseRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/definitions/{id:guid}", static async (
                [FromServices] DeleteForecastExpenseDefinitionCommandHandler deleteHandler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await deleteHandler.Handle(new DeleteForecastExpenseDefinitionCommand(id), cancellationToken);
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                return Results.NoContent();
            })
            .WithName("DeleteForecastExpenseDefinition")
            .WithDescription("Deletes a forecast expense definition")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/occurrences", static async (
                [FromServices] GetPendingForecastExpenseOccurrencesQueryHandler handler,
                [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                [AsParameters] ForecastExpenseOccurencesQuery request,
                CancellationToken cancellationToken) =>
            {
                await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);

                var query = request.ToGetPendingForecastExpenseOccurrencesQuery();
                var items = await handler.Handle(query, cancellationToken);
                var response = items.Select(o => o.ToForecastExpenseOccurrenceResponse());

                return Results.Ok(response);
            })
            .WithName("GetForecastExpenseOccurrences")
            .WithDescription("Retrieves pending forecast expense occurrences for a month")
            .Produces<List<ForecastExpenseOccurrenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/occurrences/{id:guid}", static async (
                [FromServices] DiscardForecastExpenseOccurrenceCommandHandler handler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(new DiscardForecastExpenseOccurrenceCommand(id), cancellationToken);

                return Results.NoContent();
            })
            .WithName("DiscardForecastExpenseOccurrence")
            .WithDescription("Discards a pending forecast expense occurrence")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static GetForecastExpenseRowsQuery CreateGetForecastExpenseRowsQuery(DateOnly? startDate, DateOnly? endDate)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);
        var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        return new GetForecastExpenseRowsQuery(start, end);
    }
}
