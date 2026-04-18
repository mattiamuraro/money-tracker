using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints;

public static class IncomeEndpoints
{
    internal static WebApplication AddIncomeApis(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/incomes")
            .WithTags("Incomes")
            .RequireAuthorization();

        group.MapGet("/", static async ([FromServices] IncomeService incomeService, [AsParameters] IncomeFilterQuery incomeFilterQuery, CancellationToken cancellationToken) =>
            await incomeService.GetIncomesAsync(incomeFilterQuery, cancellationToken))
            .WithName("GetIncomes")
            .WithDescription("Retrieves incomes with required month filtering and pagination")
            .Produces<PaginatedResponse<IncomeRow>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", static async ([FromServices] IncomeService incomeService, Guid id, CancellationToken cancellationToken) =>
            await incomeService.GetIncomeByIdAsync(id, cancellationToken))
            .WithName("GetIncomeById")
            .WithDescription("Retrieves a specific income by ID")
            .Produces<IncomeRow>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", static async ([FromServices] IncomeService incomeService, CreateIncomeRequest request, CancellationToken cancellationToken) =>
            await incomeService.CreateIncomeAsync(request, cancellationToken))
            .WithName("CreateIncome")
            .WithDescription("Creates a new income")
            .Accepts<CreateIncomeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", static async ([FromServices] IncomeService incomeService, Guid id, UpdateIncomeRequest request, CancellationToken cancellationToken) =>
            await incomeService.UpdateIncomeAsync(id, request, cancellationToken))
            .WithName("UpdateIncome")
            .WithDescription("Updates an income")
            .Accepts<UpdateIncomeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}", static async ([FromServices] IncomeService incomeService, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
            await incomeService.DeleteIncomeAsync(id, occurrenceAction, cancellationToken))
            .WithName("DeleteIncome")
            .WithDescription("Deletes an income")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        return app;
    }
}
