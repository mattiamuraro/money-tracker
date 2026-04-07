using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;
using MediatR;

namespace MoneyTracker.Api.Services
{
    /// <summary>
    /// Service for managing payments
    /// </summary>
    public class PaymentService
    {
        private readonly HttpContext _httpContext;
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        public PaymentService(IHttpContextAccessor httpContextAccessor, IMediator mediator, ILogger<PaymentService> logger)
        {
            _httpContext = httpContextAccessor.HttpContext!;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves payments based on the provided filter.
        /// </summary>
        public async Task<IResult> GetPaymentsAsync(PaymentFilterQuery filterQuery, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payments with filter");
                filterQuery.Validate();
                var query = new GetPaymentQuery(
                    filterQuery.StartDate,
                    filterQuery.EndDate,
                    filterQuery.CategoryFilter,
                    filterQuery.CategoryId,
                    filterQuery.MinAmount,
                    filterQuery.MaxAmount,
                    filterQuery.PageNumber ?? 1,
                    filterQuery.PageSize ?? 20,
                    filterQuery.SortBy,
                    filterQuery.SortOrder);
                var result = await _mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving payments");
            }
        }

        /// <summary>
        /// Retrieves a specific payment by ID.
        /// </summary>
        public async Task<IResult> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payment with ID: {PaymentId}", id);
                var query = new GetPaymentQuery { PageSize = 1000 };
                var result = await _mediator.Send(query, cancellationToken);
                var payment = result.Items.FirstOrDefault(p => p.Id == id);

                if (payment == null)
                    return Results.NotFound();

                return Results.Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving payment");
            }
        }

        /// <summary>
        /// Creates a new payment.
        /// </summary>
        public async Task<IResult> CreatePaymentAsync(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new payment");
                // Extract idempotency key from header if present
                var idempotencyKey = _httpContext.Request.Headers["X-Idempotency-Key"].ToString();
                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    command.IdempotencyKey = idempotencyKey;
                }

                var id = await _mediator.Send(command, cancellationToken);
                _logger.LogInformation("Payment created successfully with ID: {PaymentId}", id);
                return Results.Created($"/api/v1/payments/{id}", id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating payment");
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error creating payment");
            }
        }

        /// <summary>
        /// Updates an existing payment.
        /// </summary>
        public async Task<IResult> UpdatePaymentAsync(Guid id, UpdatePaymentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating payment with ID: {PaymentId}", id);
                command.PaymentId = id;
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment updated successfully with ID: {PaymentId}", id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment with ID: {PaymentId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error updating payment");
            }
        }

        /// <summary>
        /// Deletes a payment by ID.
        /// </summary>
        public async Task<IResult> DeletePaymentAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);
                var command = new DeletePaymentCommand { PaymentId = id };
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment deleted successfully with ID: {PaymentId}", id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting payment");
            }
        }
    }
}
