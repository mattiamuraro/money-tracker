using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;
using MoneyTracker.Api.Endpoints.ForecastExpenses.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.Api.Endpoints.ForecastExpenses;

public static class ForecastExpenseEndpoints
{
    internal static WebApplication AddForecastExpenseApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/forecast-expenses")
            .WithTags("Forecast Expenses")
            .RequireAuthorization();

        group.MapGet("/", static async (
                [FromServices] IHandler<GetForecastExpenseRowsQuery, List<ForecastExpenseRow>> getForecastRowsHandler,
                [FromQuery] DateOnly? startDate,
                [FromQuery] DateOnly? endDate,
                CancellationToken cancellationToken) =>
            {
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
                [FromServices] IHandler<GetForecastExpenseDefinitionsQuery, List<ForecastExpenseDefinitionRow>> handler,
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
                [FromServices] IHandler<CreateForecastExpenseDefinitionAndSynchronizeCommand, Guid> handler,
                CreateForecastExpenseRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateForecastExpenseDefinitionAndSynchronizeCommand
                {
                    CreateCommand = request.ToCreateForecastExpenseDefinitionCommand()
                };

                var id = await handler.Handle(command, cancellationToken);
                return Results.Created($"/api/v1/forecast-expenses/definitions/{id}", id);
            })
            .WithName("CreateForecastExpenseDefinition")
            .WithDescription("Creates a new forecast expense definition")
            .Accepts<CreateForecastExpenseRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/definitions/{id:guid}", static async (
                [FromServices] IHandler<UpdateForecastExpenseDefinitionAndSynchronizeCommand> handler,
                Guid id,
                UpdateForecastExpenseRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateForecastExpenseDefinitionAndSynchronizeCommand
                {
                    UpdateCommand = request.ToUpdateForecastExpenseDefinitionCommand(id)
                };

                await handler.Handle(command, cancellationToken);
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
                [FromServices] IHandler<DeleteForecastExpenseDefinitionAndSynchronizeCommand> handler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(new DeleteForecastExpenseDefinitionAndSynchronizeCommand { Id = id }, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteForecastExpenseDefinition")
            .WithDescription("Deletes a forecast expense definition")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/occurrences", static async (
                [FromServices] IHandler<GetPendingForecastExpenseOccurrencesQuery, List<ForecastExpenseOccurrenceRow>> handler,
                [AsParameters] ForecastExpenseOccurencesQuery request,
                CancellationToken cancellationToken) =>
            {
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
                [FromServices] IHandler<DiscardForecastExpenseOccurrenceCommand> handler,
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
