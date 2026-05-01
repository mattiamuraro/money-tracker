using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.Endpoints.Payments.ExtensionMethods;
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
            group.MapGet("/", static async ([FromServices] GetPaymentQueryHandler handler, [AsParameters] PaymentFilterQuery paymentFilterQuery, CancellationToken cancellationToken) =>
                {
                    var query = paymentFilterQuery.ToGetPaymentQuery();
                    var result = await handler.Handle(query, cancellationToken);
                    var response = result.ToPaginatedResponse();

                    return Results.Ok(response);
                })
                .WithName("GetPayments")
                .WithDescription("Retrieves all payments with required month filtering and pagination")
                .Produces<PaginatedResponse<PaymentRowResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET payment by ID
            group.MapGet("/{id:guid}", async ([FromServices] GetPaymentByIdQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var query = id.ToGetPaymentByIdQuery();
                    var payment = await handler.Handle(query, cancellationToken);
                    var response = payment.ToPaymentRowResponse();

                    return Results.Ok(response);
                })
                .WithName("GetPaymentById")
                .WithDescription("Retrieves a specific payment by ID")
                .Produces<PaymentRowResponse>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create payment
            group.MapPost("/", async (HttpContext httpContext, [FromServices] CreatePaymentCommandHandler handler, CreatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.ToCreatePaymentCommand(httpContext);
                    var id = await handler.Handle(command, cancellationToken);

                    return Results.Created($"/api/v1/payments/{id}", id);
                })
                .WithName("CreatePayment")
                .WithDescription("Creates a new payment (supports idempotency with X-Idempotency-Key header)")
                .Accepts<CreatePaymentRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update payment
            group.MapPut("/{id:guid}", async (HttpContext httpContext, [FromServices] UpdatePaymentCommandHandler handler, Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.ToUpdatePaymentCommand(id);
                    await handler.Handle(command, cancellationToken);

                    return Results.NoContent();
                })
                .WithName("UpdatePayment")
                .WithDescription("Updates an existing payment")
                .Accepts<UpdatePaymentRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // DELETE payment
            group.MapDelete("/{id:guid}", async (HttpContext httpContext, [FromServices] DeletePaymentCommandHandler handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
                {
                    var command = id.ToDeletePaymentCommand(occurrenceAction);
                    await handler.Handle(command, cancellationToken);

                    return Results.NoContent();
                })
                .WithName("DeletePayment")
                .WithDescription("Deletes a payment")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
