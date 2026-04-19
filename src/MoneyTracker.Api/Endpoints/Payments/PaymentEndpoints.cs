using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.BusinessLogic.Shared.Models;

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
                    try
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
                        return Results.Ok(result);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving payments");
                    }
                })
                .WithName("GetPayments")
                .WithDescription("Retrieves all payments with required month filtering and pagination")
                .Produces<PaginatedResponse<PaymentRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET payment by ID
            group.MapGet("/{id:guid}", async ([FromServices] GetPaymentQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var query = new GetPaymentQuery { Id = id, PageSize = 1 };
                        var result = await handler.Handle(query, cancellationToken);
                        var payment = result.Items.FirstOrDefault();
                        return payment == null ? Results.NotFound() : Results.Ok(payment);
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error retrieving payment");
                    }
                })
                .WithName("GetPaymentById")
                .WithDescription("Retrieves a specific payment by ID")
                .Produces<PaymentRow>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create payment
            group.MapPost("/", async (HttpContext httpContext, [FromServices] CreatePaymentCommandHandler handler, [FromServices] IValidator<CreatePaymentCommand> validator, CreatePaymentCommand command, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        command.CreatedById = httpContext.GetCurrentUserId();
                        var idempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();
                        if (!string.IsNullOrEmpty(idempotencyKey))
                            command.IdempotencyKey = idempotencyKey;

                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var id = await handler.Handle(command, cancellationToken);
                        return Results.Created($"/api/v1/payments/{id}", id);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { message = ex.Message });
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error creating payment");
                    }
                })
                .WithName("CreatePayment")
                .WithDescription("Creates a new payment (supports idempotency with X-Idempotency-Key header)")
                .Accepts<CreatePaymentCommand>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update payment
            group.MapPut("/{id:guid}", async (HttpContext httpContext, [FromServices] UpdatePaymentCommandHandler handler, [FromServices] IValidator<UpdatePaymentCommand> validator, Guid id, UpdatePaymentCommand command, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        command.PaymentId = id;
                        command.ModifiedById = httpContext.GetCurrentUserId();
                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var result = await handler.Handle(command, cancellationToken);
                        return !result ? Results.NotFound() : Results.NoContent();
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error updating payment");
                    }
                })
                .WithName("UpdatePayment")
                .WithDescription("Updates an existing payment")
                .Accepts<UpdatePaymentCommand>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // DELETE payment
            group.MapDelete("/{id:guid}", async (HttpContext httpContext, [FromServices] DeletePaymentCommandHandler handler, Guid id, [FromQuery] string? occurrenceAction, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        if (!occurrenceAction.TryParseOccurrenceAction(out var parsedAction))
                            return Results.BadRequest(new { message = "Occurrence action must be Auto, Reopen, or Skip." });

                        var command = new DeletePaymentCommand
                        {
                            PaymentId = id,
                            DeletedBy = httpContext.GetCurrentUserId(),
                            OccurrenceAction = parsedAction
                        };
                        var result = await handler.Handle(command, cancellationToken);
                        return !result ? Results.NotFound() : Results.NoContent();
                    }
                    catch (Exception)
                    {
                        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Error deleting payment");
                    }
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
