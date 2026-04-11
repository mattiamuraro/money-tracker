using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;

namespace MoneyTracker.Api.Endpoints
{
    public static class PaymentCategoryEndpoints
    {
        internal static WebApplication AddPaymentCategoryApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/categories")
                        .WithTags("Payment Categories")
                        .RequireAuthorization();

            // GET all categories
            group.MapGet("/", static async ([FromServices] PaymentCategoryService paymentCategoryService, CancellationToken cancellationToken) => await paymentCategoryService.GetAllCategoriesAsync(cancellationToken))
                .WithName("GetAllCategories")
                .WithDescription("Retrieves all payment categories")
                .Produces<IEnumerable<PaymentCategoryDto>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET category by ID
            group.MapGet("/{id:guid}", async ([FromServices] PaymentCategoryService paymentCategoryService, Guid id, CancellationToken cancellationToken) => await paymentCategoryService.GetCategoryByIdAsync(id, cancellationToken))
                .WithName("GetCategoryById")
                .WithDescription("Retrieves a specific payment category by ID")
                .Produces<PaymentCategoryDto>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create category
            group.MapPost("/", async (PaymentCategoryService paymentCategoryService, CreatePaymentCategoryRequest request, CancellationToken cancellationToken) => await paymentCategoryService.CreateCategoryAsync(request, cancellationToken))
                .WithName("CreateCategory")
                .WithDescription("Creates a new payment category")
                .Accepts<CreatePaymentCategoryRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update category
            group.MapPut("/{id:guid}", async (PaymentCategoryService paymentCategoryService, Guid id, UpdatePaymentCategoryRequest request, CancellationToken cancellationToken) => await paymentCategoryService.UpdateCategoryAsync(id, request, cancellationToken))
                .WithName("UpdateCategory")
                .WithDescription("Updates an existing payment category")
                .Accepts<UpdatePaymentCategoryRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // DELETE category
            group.MapDelete("/{id:guid}", async (PaymentCategoryService paymentCategoryService, Guid id, CancellationToken cancellationToken) => await paymentCategoryService.DeleteCategoryAsync(id, cancellationToken))
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
