using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;

namespace MoneyTracker.Api.Endpoints.PaymentCategories
{
    public static class PaymentCategoryEndpoints
    {
        internal static WebApplication AddPaymentCategoryApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/categories")
                        .WithTags("Payment Categories")
                        .RequireAuthorization();

            group.MapGet("/", static async ([FromServices] IHandler<GetAllCategoriesQuery, IEnumerable<PaymentCategoryDto>> handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new GetAllCategoriesQuery(), cancellationToken);
                    var response = result.Select(static category => new PaymentCategoryResponse
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Code = category.Code
                    });

                    return Results.Ok(response);
                })
                .WithName("GetAllCategories")
                .WithDescription("Retrieves all payment categories")
                .Produces<IEnumerable<PaymentCategoryResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/{id:guid}", static async ([FromServices] IHandler<GetCategoryByIdQuery, PaymentCategoryDto> handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(new GetCategoryByIdQuery(id), cancellationToken);
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
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("/", static async ([FromServices] IHandler<CreateCategoryCommand, Guid> handler, CreatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    var command = new CreateCategoryCommand
                    {
                        Name = request.Name,
                        Code = request.Code
                    };

                    var categoryId = await handler.Handle(command, cancellationToken);
                    return Results.Created($"/api/v1/categories/{categoryId}", categoryId);
                })
                .WithName("CreateCategory")
                .WithDescription("Creates a new payment category")
                .Accepts<CreatePaymentCategoryRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("/{id:guid}", static async ([FromServices] IHandler<UpdateCategoryCommand> handler, Guid id, UpdatePaymentCategoryRequest request, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCategoryCommand
                    {
                        Id = id,
                        Name = request.Name,
                        Code = request.Code
                    };

                    await handler.Handle(command, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("UpdateCategory")
                .WithDescription("Updates an existing payment category")
                .Accepts<UpdatePaymentCategoryRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("/{id:guid}", static async ([FromServices] IHandler<DeleteCategoryCommand> handler, Guid id, CancellationToken cancellationToken) =>
                {
                    await handler.Handle(new DeleteCategoryCommand(id), cancellationToken);
                    return Results.NoContent();
                })
                .WithName("DeleteCategory")
                .WithDescription("Deletes a payment category")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}

