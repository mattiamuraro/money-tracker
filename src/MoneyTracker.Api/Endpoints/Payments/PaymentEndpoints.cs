using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.Api.Endpoints.Payments
{
    public static class PaymentEndpoints
    {
        internal static WebApplication AddPaymentApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/payments")
                        .WithTags("Payments")
                        .RequireAuthorization();

            // GET all payments with pagination and filtering
            group.MapGet("/", static async ([FromServices] IHandler<GetPaymentQuery, PaginatedResponse<PaymentDto>> handler, [AsParameters] PaymentFilterQuery paymentFilterQuery, CancellationToken cancellationToken) =>
                {
                    paymentFilterQuery.Validate();
                    var (year, month) = paymentFilterQuery.GetRequiredYearMonth();
                    var query = new GetPaymentQuery(
                        paymentFilterQuery.CategoryFilter,
                        paymentFilterQuery.DescriptionFilter,
                        paymentFilterQuery.CategoryId,
                        paymentFilterQuery.MinAmount,
                        paymentFilterQuery.MaxAmount,
                        year,
                        month,
                        paymentFilterQuery.PageNumber ?? 1,
                        paymentFilterQuery.PageSize ?? 20,
                        paymentFilterQuery.SortBy,
                        paymentFilterQuery.SortOrder);

                    var result = await handler.Handle(query, cancellationToken);
                    var response = new PaginatedResponse<PaymentRowResponse>
                    {
                        Items = result.Items.Select(static payment => new PaymentRowResponse
                        {
                            Id = payment.Id,
                            Description = payment.Description,
                            PaymentCategoryId = payment.PaymentCategoryId,
                            Category = payment.Category,
                            ForecastOccurrenceId = payment.ForecastOccurrenceId,
                            ForecastExpectedDate = payment.ForecastExpectedDate,
                            Amount = payment.Amount,
                            Date = payment.Date,
                            IsOneShot = payment.IsOneShot
                        }).ToList(),
                        TotalItems = result.TotalItems,
                        PageNumber = result.PageNumber,
                        PageSize = result.PageSize
                    };

                    return Results.Ok(response);
                })
                .WithName("GetPayments")
                .WithDescription("Retrieves all payments with required month filtering and pagination")
                .Produces<PaginatedResponse<PaymentRowResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            // GET payment by ID
            group.MapGet("/{id:guid}", static async ([FromServices] IHandler<GetPaymentByIdQuery, PaymentDto> handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var payment = await handler.Handle(new GetPaymentByIdQuery(id), cancellationToken);
                    var response = new PaymentRowResponse
                    {
                        Id = payment.Id,
                        Description = payment.Description,
                        PaymentCategoryId = payment.PaymentCategoryId,
                        Category = payment.Category,
                        ForecastOccurrenceId = payment.ForecastOccurrenceId,
                        ForecastExpectedDate = payment.ForecastExpectedDate,
                        Amount = payment.Amount,
                        Date = payment.Date,
                        IsOneShot = payment.IsOneShot
                    };

                    return Results.Ok(response);
                })
                .WithName("GetPaymentById")
                .WithDescription("Retrieves a specific payment by ID")
                .Produces<PaymentRowResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            // POST create payment
            group.MapPost("/", static async (HttpContext httpContext, [FromServices] IHandler<CreatePaymentCommand, Guid> handler, CreatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    var idempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();
                    var command = new CreatePaymentCommand
                    {
                        Description = request.Description,
                        PaymentCategoryId = request.PaymentCategoryId,
                        ForecastOccurrenceId = request.ForecastOccurrenceId,
                        Amount = request.Amount,
                        Date = request.Date,
                        IsOneShot = request.IsOneShot,
                        IdempotencyKey = string.IsNullOrEmpty(idempotencyKey) ? null : idempotencyKey
                    };

                    var id = await handler.Handle(command, cancellationToken);
                    return Results.Created($"/api/v1/payments/{id}", id);
                })
                .WithName("CreatePayment")
                .WithDescription("Creates a new payment (supports idempotency with X-Idempotency-Key header)")
                .Accepts<CreatePaymentRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            // PUT update payment
            group.MapPut("/{id:guid}", static async ([FromServices] IHandler<UpdatePaymentCommand> handler, Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePaymentCommand
                    {
                        PaymentId = id,
                        Description = request.Description,
                        PaymentCategoryId = request.PaymentCategoryId,
                        Amount = request.Amount,
                        Date = request.Date,
                        IsOneShot = request.IsOneShot
                    };

                    await handler.Handle(command, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("UpdatePayment")
                .WithDescription("Updates an existing payment")
                .Accepts<UpdatePaymentRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            // DELETE payment
            group.MapDelete("/{id:guid}", static async ([FromServices] IHandler<DeletePaymentCommand> handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
                {
                    var command = new DeletePaymentCommand
                    {
                        PaymentId = id,
                        OccurrenceAction = occurrenceAction
                    };

                    await handler.Handle(command, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("DeletePayment")
                .WithDescription("Deletes a payment")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
