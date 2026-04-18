using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Models;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints
{
    public static class PaymentEndpoints
    {
        internal static WebApplication AddPaymentApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/payments")
                        .WithTags("Payments")
                        .RequireAuthorization();

            // GET all payments with pagination and filtering
            group.MapGet("/", static async ([FromServices] PaymentService paymentService, [AsParameters] PaymentFilterQuery paymentFilterQuery, CancellationToken cancellationToken) => await paymentService.GetPaymentsAsync(paymentFilterQuery, cancellationToken))
                .WithName("GetPayments")
                .WithDescription("Retrieves all payments with required month filtering and pagination")
                .Produces<PaginatedResponse<PaymentRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET payment by ID
            group.MapGet("/{id:guid}", async ([FromServices] PaymentService paymentService, Guid id, CancellationToken cancellationToken) => await paymentService.GetPaymentByIdAsync(id, cancellationToken))
                 .WithName("GetPaymentById")
                 .WithDescription("Retrieves a specific payment by ID")
                 .Produces<PaymentRow>(StatusCodes.Status200OK)
                 .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                 .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create payment
            group.MapPost("/", async (PaymentService paymentService, CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken) => await paymentService.CreatePaymentAsync(createPaymentCommand, cancellationToken))
                .WithName("CreatePayment")
                .WithDescription("Creates a new payment (supports idempotency with X-Idempotency-Key header)")
                .Accepts<CreatePaymentCommand>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update payment
            group.MapPut("/{id:guid}", async (PaymentService paymentService, Guid id, UpdatePaymentCommand updatePaymentCommand, CancellationToken cancellationToken) => await paymentService.UpdatePaymentAsync(id, updatePaymentCommand, cancellationToken))
                .WithName("UpdatePayment")
                .WithDescription("Updates an existing payment")
                .Accepts<UpdatePaymentCommand>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // DELETE payment
            group.MapDelete("/{id:guid}", async ([FromServices] PaymentService paymentService, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) => await paymentService.DeletePaymentAsync(id, occurrenceAction, cancellationToken))
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
