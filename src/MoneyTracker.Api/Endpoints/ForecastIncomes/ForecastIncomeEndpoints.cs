using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.Api.Endpoints.ForecastIncomes;

public static class ForecastIncomeEndpoints
{
    internal static WebApplication AddForecastIncomeApis(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.ForecastIncomes, "Forecast Incomes")
            .RequireReadAccess();

        group.MapGet("/", static async (
                [FromServices] IHandler<GetForecastIncomeRowsQuery, List<ForecastIncomeDto>> getForecastRowsHandler,
                [FromQuery] DateOnly? startDate,
                [FromQuery] DateOnly? endDate,
                CancellationToken cancellationToken) =>
            {
                var query = CreateGetForecastIncomeRowsQuery(startDate, endDate);
                var forecasts = await getForecastRowsHandler.Handle(query, cancellationToken);
                var response = forecasts.Select(static forecast => new ForecastIncomeRowResponse
                {
                    Id = forecast.Id,
                    ForecastDefinitionId = forecast.ForecastDefinitionId,
                    Description = forecast.Description,
                    Amount = forecast.Amount,
                    Date = forecast.Date
                });

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomes")
            .WithDescription("Retrieves forecast incomes for a given date range")
            .Produces<List<ForecastIncomeRowResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/definitions", static async (
                [FromServices] IHandler<GetForecastIncomeDefinitionsQuery, List<ForecastIncomeDefinitionDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var definitions = await handler.Handle(new GetForecastIncomeDefinitionsQuery(), cancellationToken);
                var response = definitions.Select(static definition => new ForecastIncomeDefinitionResponse
                {
                    Id = definition.Id,
                    ForecastRecurrenceRuleTypeId = definition.ForecastRecurrenceRuleTypeId,
                    Description = definition.Description,
                    Amount = definition.Amount,
                    RecurrenceStart = definition.RecurrenceStart,
                    RecurrenceEnd = definition.RecurrenceEnd,
                    Interval = definition.Interval
                });

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomeDefinitions")
            .WithDescription("Retrieves all forecast income definitions")
            .Produces<List<ForecastIncomeDefinitionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/definitions", static async (
                [FromServices] IHandler<CreateForecastIncomeDefinitionAndSynchronizeCommand, Guid> handler,
                CreateForecastIncomeRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateForecastIncomeDefinitionAndSynchronizeCommand
                {
                    CreateCommand = new CreateForecastIncomeDefinitionCommand
                    {
                        ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                        Description = request.Description,
                        Amount = request.Amount,
                        RecurrenceStart = request.RecurrenceStart,
                        RecurrenceEnd = request.RecurrenceEnd,
                        Interval = request.Interval
                    }
                };

                var id = await handler.Handle(command, cancellationToken);
                return ApiEndpointConventions.CreatedResource(ApiRoutes.ForecastIncomeDefinitions, id);
            })
            .RequireWriteAccess()
            .WithName("CreateForecastIncomeDefinition")
            .WithDescription("Creates a new forecast income definition")
            .Accepts<CreateForecastIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/definitions/{id:guid}", static async (
                [FromServices] IHandler<UpdateForecastIncomeDefinitionAndSynchronizeCommand> handler,
                Guid id,
                UpdateForecastIncomeRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateForecastIncomeDefinitionAndSynchronizeCommand
                {
                    UpdateCommand = new UpdateForecastIncomeDefinitionCommand
                    {
                        Id = id,
                        ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                        Description = request.Description,
                        Amount = request.Amount,
                        RecurrenceStart = request.RecurrenceStart,
                        RecurrenceEnd = request.RecurrenceEnd,
                        Interval = request.Interval
                    }
                };

                await handler.Handle(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireWriteAccess()
            .WithName("UpdateForecastIncomeDefinition")
            .WithDescription("Updates an existing forecast income definition")
            .Accepts<UpdateForecastIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/definitions/{id:guid}", static async (
                [FromServices] IHandler<DeleteForecastIncomeDefinitionAndSynchronizeCommand> handler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(new DeleteForecastIncomeDefinitionAndSynchronizeCommand { Id = id }, cancellationToken);
                return Results.NoContent();
            })
            .RequireWriteAccess()
            .WithName("DeleteForecastIncomeDefinition")
            .WithDescription("Deletes a forecast income definition")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/occurrences", static async (
                [FromServices] IHandler<GetPendingForecastIncomeOccurrencesQuery, List<ForecastIncomeOccurrenceDto>> handler,
                [AsParameters] ForecastIncomeOccurrencesQuery request,
                CancellationToken cancellationToken) =>
            {
                var (year, month) = request.GetRequiredYearMonth();
                var items = await handler.Handle(new GetPendingForecastIncomeOccurrencesQuery(year, month), cancellationToken);
                var response = items.Select(static occurrence => new ForecastIncomeOccurrenceResponse
                {
                    Id = occurrence.Id,
                    ForecastDefinitionId = occurrence.ForecastDefinitionId,
                    Description = occurrence.Description,
                    Amount = occurrence.Amount,
                    ExpectedDate = occurrence.ExpectedDate
                });

                return Results.Ok(response);
            })
            .WithName("GetForecastIncomeOccurrences")
            .WithDescription("Retrieves pending forecast income occurrences for a month")
            .Produces<List<ForecastIncomeOccurrenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/occurrences/{id:guid}", static async (
                [FromServices] IHandler<DiscardForecastIncomeOccurrenceCommand> handler,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                await handler.Handle(new DiscardForecastIncomeOccurrenceCommand(id), cancellationToken);

                return Results.NoContent();
            })
            .RequireWriteAccess()
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
