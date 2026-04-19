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
                        return Results.BadRequest(new ErrorResponse { Message = "End date must be greater than or equal to start date." });

                    if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 366)
                        return Results.BadRequest(new ErrorResponse { Message = "Date range cannot exceed 366 days." });

                    await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                    var forecasts = await getForecastRowsHandler.Handle(new GetForecastRowsQuery(start, end), cancellationToken);
                    var response = forecasts.Select(f => new ForecastRowResponse
                    {
                        Id = f.Id,
                        ForecastDefinitionId = f.ForecastDefinitionId,
                        Description = f.Description,
                        Amount = f.Amount,
                        Date = f.Date,
                        IsIncome = f.IsIncome,
                        PaymentCategoryId = f.PaymentCategoryId,
                        Category = f.Category
                    });
                    return Results.Ok(response);
                })
                .WithName("GetForecasts")
                .WithDescription("Retrieves forecasts for a given date range")
                .Produces<List<ForecastRowResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions", static async ([FromServices] GetForecastDefinitionsQueryHandler handler, CancellationToken cancellationToken) =>
                {
                    var definitions = await handler.Handle(new GetForecastDefinitionsQuery(), cancellationToken);
                    var response = definitions.Select(d => new ForecastDefinitionResponse
                    {
                        Id = d.Id,
                        ForecastRecurrenceRuleTypeId = d.ForecastRecurrenceRuleTypeId,
                        Description = d.Description,
                        Amount = d.Amount,
                        RecurrenceStart = d.RecurrenceStart,
                        RecurrenceEnd = d.RecurrenceEnd,
                        Interval = d.Interval,
                        IsIncome = d.IsIncome,
                        PaymentCategoryId = d.PaymentCategoryId
                    });
                    return Results.Ok(response);
                })
                .WithName("GetForecastDefinitions")
                .WithDescription("Retrieves all forecast definitions")
                .Produces<List<ForecastDefinitionResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions/{id:guid}", static async ([FromServices] GetForecastDefinitionByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var definition = await handler.Handle(new GetForecastDefinitionByIdQuery(id), cancellationToken);
                    if (definition == null)
                        return Results.NotFound();

                    var response = new ForecastDefinitionResponse
                    {
                        Id = definition.Id,
                        ForecastRecurrenceRuleTypeId = definition.ForecastRecurrenceRuleTypeId,
                        Description = definition.Description,
                        Amount = definition.Amount,
                        RecurrenceStart = definition.RecurrenceStart,
                        RecurrenceEnd = definition.RecurrenceEnd,
                        Interval = definition.Interval,
                        IsIncome = definition.IsIncome,
                        PaymentCategoryId = definition.PaymentCategoryId
                    };
                    return Results.Ok(response);
                })
                .WithName("GetForecastDefinitionById")
                .WithDescription("Retrieves a specific forecast definition by ID")
                .Produces<ForecastDefinitionResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapPost("/definitions", static async (
                    [FromServices] CreateForecastDefinitionCommandHandler createHandler,
                    [FromServices] SynchronizeForecastOccurrencesCommandHandler synchronizeHandler,
                    CreateForecastRequest request,
                    CancellationToken cancellationToken) =>
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
                    var deleted = await deleteHandler.Handle(new DeleteForecastDefinitionCommand(id), cancellationToken);
                    if (!deleted)
                        return Results.NotFound();

                    await synchronizeHandler.Handle(new SynchronizeForecastOccurrencesCommand(), cancellationToken);
                    return Results.NoContent();
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
                        var response = items.Select(o => new ForecastOccurrenceResponse
                        {
                            Id = o.Id,
                            ForecastDefinitionId = o.ForecastDefinitionId,
                            Description = o.Description,
                            Amount = o.Amount,
                            ExpectedDate = o.ExpectedDate,
                            IsIncome = o.IsIncome,
                            PaymentCategoryId = o.PaymentCategoryId,
                            Category = o.Category
                        });
                        return Results.Ok(response);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                    }
                })
                .WithName("GetForecastOccurrences")
                .WithDescription("Retrieves pending forecast occurrences for a month and type")
                .Produces<List<ForecastOccurrenceResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/occurrences/{id:guid}", static async ([FromServices] DiscardPendingForecastOccurrenceCommandHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var discarded = await handler.Handle(new DiscardPendingForecastOccurrenceCommand(id), cancellationToken);
                    return !discarded ? Results.NotFound() : Results.NoContent();
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
