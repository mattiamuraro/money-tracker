using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints.PaymentCategories
{
    public static class PaymentCategoryEndpoints
    {
        internal static WebApplication AddPaymentCategoryApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/categories")
                        .WithTags("Payment Categories")
                        .RequireAuthorization();

            // GET all categories
            group.MapGet("/", static async ([FromServices] GetAllCategoriesQueryHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new GetAllCategoriesQuery(), cancellationToken);
                    var response = result.Select(c => new PaymentCategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code
                    });
                    return Results.Ok(response);
                })
                .WithName("GetAllCategories")
                .WithDescription("Retrieves all payment categories")
                .Produces<IEnumerable<PaymentCategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET category by ID
            group.MapGet("/{id:guid}", async ([FromServices] GetCategoryByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new GetCategoryByIdQuery(id), cancellationToken);
                    if (result == null)
                        return Results.NotFound();

                    var response = new PaymentCategoryResponse
                    {
                        Id = result.Id,
                        Name = result.Name,
                        Code = result.Code
                    };
                    return Results.Ok(response);
                })
                .WithName("GetCategoryById")
                .WithDescription("Retrieves a specific payment category by ID")
                .Produces<PaymentCategoryResponse>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create category
            group.MapPost("/", async ([FromServices] CreateCategoryCommandHandler handler, [FromServices] IValidator<CreateCategoryCommand> validator, CreatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new CreateCategoryCommand { Name = request.Name, Code = request.Code };
                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var categoryId = await handler.Handle(command, cancellationToken);
                        return Results.Created($"/api/v1/categories/{categoryId}", categoryId);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
                })
                .WithName("CreateCategory")
                .WithDescription("Creates a new payment category")
                .Accepts<CreatePaymentCategoryRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update category
            group.MapPut("/{id:guid}", async ([FromServices] UpdateCategoryCommandHandler handler, [FromServices] IValidator<UpdateCategoryCommand> validator, Guid id, UpdatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new UpdateCategoryCommand { Id = id, Name = request.Name, Code = request.Code };
                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var result = await handler.Handle(command, cancellationToken);
                        return !result ? Results.NotFound() : Results.NoContent();
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
                })
                .WithName("UpdateCategory")
                .WithDescription("Updates an existing payment category")
                .Accepts<UpdatePaymentCategoryRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // DELETE category
            group.MapDelete("/{id:guid}", async ([FromServices] DeleteCategoryCommandHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new DeleteCategoryCommand(id), cancellationToken);
                    return !result ? Results.NotFound() : Results.NoContent();
                })
                .WithName("DeleteCategory")
                .WithDescription("Deletes a payment category")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
