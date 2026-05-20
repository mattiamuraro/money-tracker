using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.Api.Endpoints.Incomes;

public static class IncomeEndpoints
{
    internal static WebApplication AddIncomeApis(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Incomes, "Incomes")
            .RequireReadAccess();

        group.MapGet("/", static async ([FromServices] IHandler<GetIncomeQuery, PaginatedResponse<IncomeDto>> handler, [AsParameters] IncomeFilterQuery incomeFilterQuery, CancellationToken cancellationToken) =>
            {
                incomeFilterQuery.Validate();
                var (year, month) = incomeFilterQuery.GetRequiredYearMonth();
                var query = new GetIncomeQuery(
                    incomeFilterQuery.DescriptionFilter,
                    incomeFilterQuery.MinAmount,
                    incomeFilterQuery.MaxAmount,
                    year,
                    month,
                    incomeFilterQuery.GetPageNumber(),
                    incomeFilterQuery.GetPageSize(),
                    incomeFilterQuery.SortBy,
                    incomeFilterQuery.SortOrder);

                var result = await handler.Handle(query, cancellationToken);
                var response = new PaginatedResponse<IncomeRowResponse>
                {
                    Items = result.Items.Select(static income => new IncomeRowResponse
                    {
                        Id = income.Id,
                        Description = income.Description,
                        ForecastOccurrenceId = income.ForecastOccurrenceId,
                        ForecastExpectedDate = income.ForecastExpectedDate,
                        Amount = income.Amount,
                        Date = income.Date
                    }).ToList(),
                    TotalItems = result.TotalItems,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                return Results.Ok(response);
            })
            .WithName("GetIncomes")
            .WithDescription("Retrieves incomes with required month filtering and pagination")
            .Produces<PaginatedResponse<IncomeRowResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", static async ([FromServices] IHandler<GetIncomeByIdQuery, IncomeDto> handler, Guid id, CancellationToken cancellationToken) =>
            {
                var income = await handler.Handle(new GetIncomeByIdQuery(id), cancellationToken);
                var response = new IncomeRowResponse
                {
                    Id = income.Id,
                    Description = income.Description,
                    ForecastOccurrenceId = income.ForecastOccurrenceId,
                    ForecastExpectedDate = income.ForecastExpectedDate,
                    Amount = income.Amount,
                    Date = income.Date
                };

                return Results.Ok(response);
            })
            .WithName("GetIncomeById")
            .WithDescription("Retrieves a specific income by ID")
            .Produces<IncomeRowResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", static async (HttpContext httpContext, [FromServices] IHandler<CreateIncomeCommand, Guid> handler, CreateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                var command = new CreateIncomeCommand
                {
                    Description = request.Description,
                    ForecastOccurrenceId = request.ForecastOccurrenceId,
                    Amount = request.Amount,
                    Date = request.Date,
                    IdempotencyKey = httpContext.GetIdempotencyKey(request.IdempotencyKey)
                };

                var id = await handler.Handle(command, cancellationToken);
                return ApiEndpointConventions.CreatedResource(ApiRoutes.Incomes, id);
            })
            .RequireWriteAccess()
            .WithName("CreateIncome")
            .WithDescription("Creates a new income")
            .Accepts<CreateIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", static async ([FromServices] IHandler<UpdateIncomeCommand> handler, Guid id, UpdateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                var command = new UpdateIncomeCommand
                {
                    IncomeId = id,
                    Description = request.Description,
                    Amount = request.Amount,
                    Date = request.Date
                };

                await handler.Handle(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireWriteAccess()
            .WithName("UpdateIncome")
            .WithDescription("Updates an income")
            .Accepts<UpdateIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}", static async ([FromServices] IHandler<DeleteIncomeCommand> handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
            {
                var command = new DeleteIncomeCommand
                {
                    IncomeId = id,
                    OccurrenceAction = occurrenceAction
                };

                await handler.Handle(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireWriteAccess()
            .WithName("DeleteIncome")
            .WithDescription("Deletes an income")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}

