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
                        var response = new PaginatedResponse<PaymentRowResponse>
                        {
                            Items = result.Items.Select(p => new PaymentRowResponse
                            {
                                Id = p.Id,
                                Description = p.Description,
                                PaymentCategoryId = p.PaymentCategoryId,
                                Category = p.Category,
                                ForecastOccurrenceId = p.ForecastOccurrenceId,
                                ForecastExpectedDate = p.ForecastExpectedDate,
                                Amount = p.Amount,
                                Date = p.Date,
                                IsOneShot = p.IsOneShot
                            }).ToList(),
                            TotalItems = result.TotalItems,
                            PageNumber = result.PageNumber,
                            PageSize = result.PageSize
                        };
                        return Results.Ok(response);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                    }
                })
                .WithName("GetPayments")
                .WithDescription("Retrieves all payments with required month filtering and pagination")
                .Produces<PaginatedResponse<PaymentRowResponse>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // GET payment by ID
            group.MapGet("/{id:guid}", async ([FromServices] GetPaymentQueryHandler handler, Guid id, CancellationToken cancellationToken) =>
                {
                    var query = new GetPaymentQuery { Id = id, PageSize = 1 };
                    var result = await handler.Handle(query, cancellationToken);
                    var payment = result.Items.FirstOrDefault();
                    if (payment == null)
                        return Results.NotFound();

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
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // POST create payment
            group.MapPost("/", async (HttpContext httpContext, [FromServices] CreatePaymentCommandHandler handler, [FromServices] IValidator<CreatePaymentCommand> validator, CreatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    try
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
                            CreatedById = httpContext.GetCurrentUserId(),
                            IdempotencyKey = string.IsNullOrEmpty(idempotencyKey) ? null : idempotencyKey
                        };
                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var id = await handler.Handle(command, cancellationToken);
                        return Results.Created($"/api/v1/payments/{id}", id);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
                })
                .WithName("CreatePayment")
                .WithDescription("Creates a new payment (supports idempotency with X-Idempotency-Key header)")
                .Accepts<CreatePaymentRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            // PUT update payment
            group.MapPut("/{id:guid}", async (HttpContext httpContext, [FromServices] UpdatePaymentCommandHandler handler, [FromServices] IValidator<UpdatePaymentCommand> validator, Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var command = new UpdatePaymentCommand
                        {
                            PaymentId = id,
                            Description = request.Description,
                            PaymentCategoryId = request.PaymentCategoryId,
                            Amount = request.Amount,
                            Date = request.Date,
                            IsOneShot = request.IsOneShot,
                            ModifiedById = httpContext.GetCurrentUserId()
                        };
                        await validator.ValidateAndThrowAsync(command, cancellationToken);
                        var result = await handler.Handle(command, cancellationToken);
                        return !result ? Results.NotFound() : Results.NoContent();
                    }
                    catch (ValidationException ex)
                    {
                        return Results.ValidationProblem(ex.ToValidationErrors());
                    }
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
                    if (!occurrenceAction.TryParseOccurrenceAction(out var parsedAction))
                        return Results.BadRequest(new ErrorResponse { Message = "Occurrence action must be Auto, Reopen, or Skip." });

                    var command = new DeletePaymentCommand
                    {
                        PaymentId = id,
                        DeletedBy = httpContext.GetCurrentUserId(),
                        OccurrenceAction = parsedAction
                    };
                    var result = await handler.Handle(command, cancellationToken);
                    return !result ? Results.NotFound() : Results.NoContent();
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
