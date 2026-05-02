using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.Api.Endpoints.Forecasts.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;

namespace MoneyTracker.Api.Endpoints.Forecasts
{
    public static class ForecastEndpoints
    {
        internal static WebApplication AddForecastApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/forecasts")
                        .WithTags("Forecasts")
                        .RequireAuthorization();

            group.MapGet("/", static async (
                    [FromServices] GetForecastRowsQueryHandler getForecastRowsHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    [FromQuery] DateOnly? startDate,
                    [FromQuery] DateOnly? endDate,
                    CancellationToken cancellationToken) =>
            {
                var query = startDate.GetForecastRowsQuery(endDate);
                var forecasts = await getForecastRowsHandler.Handle(query, cancellationToken);
                var response = forecasts.Select(f => f.ToForecastRowResponse());

                return Results.Ok(response);
            })
                .WithName("GetForecasts")
                .WithDescription("Retrieves forecasts for a given date range")
                .Produces<List<ForecastRowResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions", static async ([FromServices] GetForecastDefinitionsQueryHandler handler, CancellationToken cancellationToken) =>
                {
                    var definitions = await handler.Handle(new GetForecastDefinitionsQuery(), cancellationToken);
                    var response = definitions.Select(d => d.ToForecastDefinitionResponse());

                    return Results.Ok(response);
                })
                .WithName("GetForecastDefinitions")
                .WithDescription("Retrieves all forecast definitions")
                .Produces<List<ForecastDefinitionResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions/{id:guid}", static async ([FromServices] GetForecastDefinitionByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var definition = await handler.Handle(new GetForecastDefinitionByIdQuery(id), cancellationToken);
                    var response = definition.ToForecastDefinitionResponse();

                    return Results.Ok(response);
                })
                .WithName("GetForecastDefinitionById")
                .WithDescription("Retrieves a specific forecast definition by ID")
                .Produces<ForecastDefinitionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("/definitions", static async (
                    [FromServices] CreateForecastDefinitionCommandHandler createHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    CreateForecastRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = request.ToCreateForecastDefinitionCommand();
                    var id = await createHandler.Handle(command, cancellationToken);

                    return Results.Created($"/api/v1/forecasts/definitions/{id}", id);
                })
                .WithName("CreateForecastDefinition")
                .WithDescription("Creates a new forecast definition")
                .Accepts<CreateForecastRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("/definitions/{id:guid}", static async (
                    [FromServices] UpdateForecastDefinitionCommandHandler updateHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    Guid id,
                    UpdateForecastRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = request.ToUpdateForecastDefinitionCommand(id);
                    await updateHandler.Handle(command, cancellationToken);

                    return Results.NoContent();
                })
                .WithName("UpdateForecastDefinition")
                .WithDescription("Updates an existing forecast definition")
                .Accepts<UpdateForecastRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("/definitions/{id:guid}", static async (
                    [FromServices] DeleteForecastDefinitionCommandHandler deleteHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    Guid id,
                    CancellationToken cancellationToken) =>
                {
                    var command = id.ToDeleteForecastDefinitionCommand();
                    await deleteHandler.Handle(command, cancellationToken);
                    
                    return Results.NoContent();
                })
                .WithName("DeleteForecastDefinition")
                .WithDescription("Deletes a forecast definition")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/occurrences", static async (
                    [FromServices] GetPendingForecastOccurrencesQueryHandler handler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    [AsParameters] ForecastOccurrenceQuery requesst,
                    CancellationToken cancellationToken) =>
                {
                    var query = requesst.ToGetPendingForecastOccurrencesQuery();
                    var items = await handler.Handle(query, cancellationToken);
                    var response = items.Select(o => o.ToForecastOccurrenceResponse());

                    return Results.Ok(response);
                })
                .WithName("GetForecastOccurrences")
                .WithDescription("Retrieves pending forecast occurrences for a month and type")
                .Produces<List<ForecastOccurrenceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("/occurrences/{id:guid}", static async ([FromServices] DiscardPendingForecastOccurrenceCommandHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var command = id.ToDiscardPendingForecastOccurrenceCommand();
                    await handler.Handle(command, cancellationToken);

                    return Results.NoContent();
                })
                .WithName("DiscardForecastOccurrence")
                .WithDescription("Discards a pending forecast occurrence")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
