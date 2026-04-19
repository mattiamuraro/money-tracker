using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;
using MoneyTracker.BusinessLogic.Shared.Models;

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
                    var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);
                    var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

                    if (end < start)
                        return Results.BadRequest(new { message = "End date must be greater than or equal to start date." });

                    if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 366)
                        return Results.BadRequest(new { message = "Date range cannot exceed 366 days." });

                    try
                    {
                        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                        var forecasts = await getForecastRowsHandler.Handle(new GetForecastRowsQuery(start, end), cancellationToken);
                        return Results.Ok(forecasts);
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving forecast rows");
                    }
                })
                .WithName("GetForecasts")
                .WithDescription("Retrieves forecasts for a given date range")
                .Produces<List<ForecastRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions", static async ([FromServices] GetForecastDefinitionsQueryHandler handler, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var definitions = await handler.Handle(new GetForecastDefinitionsQuery(), cancellationToken);
                        return Results.Ok(definitions);
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving forecast definitions");
                    }
                })
                .WithName("GetForecastDefinitions")
                .WithDescription("Retrieves all forecast definitions")
                .Produces<List<ForecastDefinitionDto>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions/{id:guid}", static async ([FromServices] GetForecastDefinitionByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var definition = await handler.Handle(new GetForecastDefinitionByIdQuery(id), cancellationToken);
                        return definition == null ? Results.NotFound() : Results.Ok(definition);
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving forecast definition");
                    }
                })
                .WithName("GetForecastDefinitionById")
                .WithDescription("Retrieves a specific forecast definition by ID")
                .Produces<ForecastDefinitionDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapPost("/definitions", static async (
                    [FromServices] CreateForecastDefinitionCommandHandler createHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    CreateForecastRequest request,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new CreateForecastDefinitionCommand
                        {
                            ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                            Description = request.Description,
                            Amount = request.Amount,
                            RecurrenceStart = request.RecurrenceStart,
                            RecurrenceEnd = request.RecurrenceEnd,
                            Interval = request.Interval,
                            IsIncome = request.IsIncome,
                            PaymentCategoryId = request.PaymentCategoryId
                        };
                        var id = await createHandler.Handle(command, cancellationToken);
                        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                        return Results.Created($"/api/v1/forecasts/definitions/{id}", id);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error creating forecast definition");
                    }
                })
                .WithName("CreateForecastDefinition")
                .WithDescription("Creates a new forecast definition")
                .Accepts<CreateForecastRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapPut("/definitions/{id:guid}", static async (
                    [FromServices] UpdateForecastDefinitionCommandHandler updateHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    Guid id,
                    UpdateForecastRequest request,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new UpdateForecastDefinitionCommand
                        {
                            Id = id,
                            ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                            Description = request.Description,
                            Amount = request.Amount,
                            RecurrenceStart = request.RecurrenceStart,
                            RecurrenceEnd = request.RecurrenceEnd,
                            Interval = request.Interval,
                            IsIncome = request.IsIncome,
                            PaymentCategoryId = request.PaymentCategoryId
                        };
                        var updated = await updateHandler.Handle(command, cancellationToken);
                        if (!updated)
                            return Results.NotFound();

                        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error updating forecast definition");
                    }
                })
                .WithName("UpdateForecastDefinition")
                .WithDescription("Updates an existing forecast definition")
                .Accepts<UpdateForecastRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/definitions/{id:guid}", static async (
                    [FromServices] DeleteForecastDefinitionCommandHandler deleteHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    Guid id,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var deleted = await deleteHandler.Handle(new DeleteForecastDefinitionCommand(id), cancellationToken);
                        if (!deleted)
                            return Results.NotFound();

                        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                        return Results.NoContent();
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error deleting forecast definition");
                    }
                })
                .WithName("DeleteForecastDefinition")
                .WithDescription("Deletes a forecast definition")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/occurrences", static async (
                    [FromServices] GetPendingForecastOccurrencesQueryHandler handler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    [AsParameters] ForecastOccurrenceQuery query,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var (year, month) = query.GetRequiredYearMonth();
                        await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                        var items = await handler.Handle(new GetPendingForecastOccurrencesQuery(year, month, query.IsIncome), cancellationToken);
                        return Results.Ok(items);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving forecast occurrences");
                    }
                })
                .WithName("GetForecastOccurrences")
                .WithDescription("Retrieves pending forecast occurrences for a month and type")
                .Produces<List<ForecastOccurrenceRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/occurrences/{id:guid}", static async ([FromServices] DiscardPendingForecastOccurrenceCommandHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var discarded = await handler.Handle(new DiscardPendingForecastOccurrenceCommand(id), cancellationToken);
                        return !discarded ? Results.NotFound() : Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error discarding forecast occurrence");
                    }
                })
                .WithName("DiscardForecastOccurrence")
                .WithDescription("Discards a pending forecast occurrence")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
