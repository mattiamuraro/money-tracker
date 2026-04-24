using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.Api.Endpoints.PaymentCategories.ExtensionMethods;
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
                    var response = result.ToPaymentCategoryResponses();

                    return Results.Ok(response);
                })
                .WithName("GetAllCategories")
                .WithDescription("Retrieves all payment categories")
                .Produces<IEnumerable<PaymentCategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET category by ID
            group.MapGet("/{id:guid}", async ([FromServices] GetCategoryByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var query = id.ToGetCategoryByIdQuery();
                    var result = await handler.Handle(query, cancellationToken);
                    var response = result.ToPaymentCategoryResponse();

                    return Results.Ok(response);
                })
                .WithName("GetCategoryById")
                .WithDescription("Retrieves a specific payment category by ID")
                .Produces<PaymentCategoryResponse>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create category
            group.MapPost("/", async ([FromServices] CreateCategoryCommandHandler handler, CreatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.ToCreateCategoryCommand();
                    var categoryId = await handler.Handle(command, cancellationToken);

                    return Results.Created($"/api/v1/categories/{categoryId}", categoryId);
                })
                .WithName("CreateCategory")
                .WithDescription("Creates a new payment category")
                .Accepts<CreatePaymentCategoryRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update category
            group.MapPut("/{id:guid}", async ([FromServices] UpdateCategoryCommandHandler handler, Guid id, UpdatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.ToUpdateCategoryCommand(id);
                    await handler.Handle(command, cancellationToken);

                    return Results.NoContent();
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
                    var command = id.ToDeleteCategoryCommand();
                    await handler.Handle(command, cancellationToken);

                    return Results.NoContent();
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
