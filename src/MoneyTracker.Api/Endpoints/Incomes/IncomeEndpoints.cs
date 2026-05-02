using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.Api.Endpoints.Incomes.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.Data;

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
                var query = incomeFilterQuery.ToGetIncomeQuery();
                var result = await handler.Handle(query, cancellationToken);
                var response = result.ToPaginatedResponse();

                return Results.Ok(response);
            })
            .WithName("GetIncomes")
            .WithDescription("Retrieves incomes with required month filtering and pagination")
            .Produces<PaginatedResponse<IncomeRowResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", static async ([FromServices] GetIncomeByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
            {
                var query = id.ToGetIncomeByIdQuery();
                var income = await handler.Handle(query, cancellationToken);
                var response = income.ToIncomeRowResponse();

                return Results.Ok(response);
            })
            .WithName("GetIncomeById")
            .WithDescription("Retrieves a specific income by ID")
            .Produces<IncomeRowResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", static async (HttpContext httpContext, [FromServices] CreateIncomeCommandHandler handler, CreateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                var command = request.ToCreateIncomeCommand(httpContext);
                var id = await handler.Handle(command, cancellationToken);

                return Results.Created($"/api/v1/incomes/{id}", id);
            })
            .WithName("CreateIncome")
            .WithDescription("Creates a new income")
            .Accepts<CreateIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", static async (HttpContext httpContext, [FromServices] UpdateIncomeCommandHandler handler, Guid id, UpdateIncomeRequest request, CancellationToken cancellationToken) =>
            {
                var command = request.ToUpdateIncomeCommand(id);
                await handler.Handle(command, cancellationToken);

                return Results.NoContent();
            })
            .WithName("UpdateIncome")
            .WithDescription("Updates an income")
            .Accepts<UpdateIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}", static async (HttpContext httpContext, [FromServices] DeleteIncomeCommandHandler handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
            {
                var command = id.ToDeleteIncomeCommand(occurrenceAction);
                await handler.Handle(command, cancellationToken);

                return Results.NoContent();
            })
            .WithName("DeleteIncome")
            .WithDescription("Deletes an income")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}
