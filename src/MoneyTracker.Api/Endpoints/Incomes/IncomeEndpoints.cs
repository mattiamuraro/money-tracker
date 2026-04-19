using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Shared.Models;
using ApiIncomeRow = MoneyTracker.Api.Endpoints.Incomes.Contracts.IncomeRow;
using BusinessIncomeRow = MoneyTracker.BusinessLogic.Features.Incomes.GetIncome.IncomeRow;

namespace MoneyTracker.Api.Endpoints.Incomes;

public static class IncomeEndpoints
{
    internal static WebApplication AddIncomeApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/incomes")
            .WithTags("Incomes")
            .RequireAuthorization();

        group.MapGet("/", static async ([FromServices] GetIncomeQueryHandler handler, [AsParameters] IncomeFilterQuery incomeFilterQuery, CancellationToken cancellationToken) =>
            {
                try
                {
                    incomeFilterQuery.Validate();
                    var (year, month) = incomeFilterQuery.GetRequiredYearMonth();
                    var query = new GetIncomeQuery(
                        incomeFilterQuery.DescriptionFilter,
                        incomeFilterQuery.MinAmount,
                        incomeFilterQuery.MaxAmount,
                        year,
                        month,
                        incomeFilterQuery.PageNumber ?? 1,
                        incomeFilterQuery.PageSize ?? 20,
                        incomeFilterQuery.SortBy,
                        incomeFilterQuery.SortOrder);
                    var result = await handler.Handle(query, cancellationToken);
                    var response = new PaginatedResponse<ApiIncomeRow>
                    {
                        Items = result.Items.Select(i => new ApiIncomeRow
                        {
                            Id = i.Id,
                            Description = i.Description,
                            ForecastOccurrenceId = i.ForecastOccurrenceId,
                            ForecastExpectedDate = i.ForecastExpectedDate,
                            Amount = i.Amount,
                            Date = i.Date
                        }).ToList(),
                        TotalItems = result.TotalItems,
                        PageNumber = result.PageNumber,
                        PageSize = result.PageSize
                    };
                    return Results.Ok(response);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithName("GetIncomes")
            .WithDescription("Retrieves incomes with required month filtering and pagination")
            .Produces<PaginatedResponse<ApiIncomeRow>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", static async ([FromServices] GetIncomeQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
            {
                var query = new GetIncomeQuery { Id = id, PageSize = 1 };
                var result = await handler.Handle(query, cancellationToken);
                var income = result.Items.FirstOrDefault();
                if (income is null)
                    return Results.NotFound();

                var response = new ApiIncomeRow
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
            .Produces<ApiIncomeRow>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", static async (HttpContext httpContext, [FromServices] CreateIncomeCommandHandler handler, [FromServices] IValidator<CreateIncomeCommand> validator, CreateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                try
                {
                    var idempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();
                    var command = new CreateIncomeCommand
                    {
                        Description = request.Description,
                        ForecastOccurrenceId = request.ForecastOccurrenceId,
                        Amount = request.Amount,
                        Date = request.Date,
                        CreatedById = httpContext.GetCurrentUserId(),
                        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? request.IdempotencyKey : idempotencyKey
                    };
                    await validator.ValidateAndThrowAsync(command, cancellationToken);
                    var id = await handler.Handle(command, cancellationToken);
                    return Results.Created($"/api/v1/incomes/{id}", id);
                }
                catch (ValidationException ex)
                {
                    return Results.ValidationProblem(ex.ToValidationErrors());
                }
            })
            .WithName("CreateIncome")
            .WithDescription("Creates a new income")
            .Accepts<CreateIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", static async (HttpContext httpContext, [FromServices] UpdateIncomeCommandHandler handler, [FromServices] IValidator<UpdateIncomeCommand> validator, Guid id, UpdateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                try
                {
                    var command = new UpdateIncomeCommand
                    {
                        IncomeId = id,
                        Description = request.Description,
                        Amount = request.Amount,
                        Date = request.Date,
                        ModifiedById = httpContext.GetCurrentUserId()
                    };
                    await validator.ValidateAndThrowAsync(command, cancellationToken);
                    var updated = await handler.Handle(command, cancellationToken);
                    return updated ? Results.NoContent() : Results.NotFound();
                }
                catch (ValidationException ex)
                {
                    return Results.ValidationProblem(ex.ToValidationErrors());
                }
            })
            .WithName("UpdateIncome")
            .WithDescription("Updates an income")
            .Accepts<UpdateIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}", static async (HttpContext httpContext, [FromServices] DeleteIncomeCommandHandler handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
            {
                if (!occurrenceAction.TryParseOccurrenceAction(out var parsedAction))
                    return Results.BadRequest(new ErrorResponse { Message = "Occurrence action must be Auto, Reopen, or Skip." });

                var command = new DeleteIncomeCommand
                {
                    IncomeId = id,
                    DeletedBy = httpContext.GetCurrentUserId(),
                    OccurrenceAction = parsedAction
                };
                var deleted = await handler.Handle(command, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteIncome")
            .WithDescription("Deletes an income")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        return app;
    }
}
