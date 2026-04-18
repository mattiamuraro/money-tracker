using FluentValidation;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.Data;
using System.Security.Claims;

namespace MoneyTracker.Api.Services
{
    /// <summary>
    /// Service for managing payments
    /// </summary>
    public class PaymentService
    {
        private readonly HttpContext _httpContext;
        private readonly GetPaymentQueryHandler _getPaymentQueryHandler;
        private readonly CreatePaymentCommandHandler _createPaymentCommandHandler;
        private readonly UpdatePaymentCommandHandler _updatePaymentCommandHandler;
        private readonly DeletePaymentCommandHandler _deletePaymentCommandHandler;
        private readonly IValidator<CreatePaymentCommand> _createPaymentCommandValidator;
        private readonly IValidator<UpdatePaymentCommand> _updatePaymentCommandValidator;
        private readonly ILogger<PaymentService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        public PaymentService(
            IHttpContextAccessor httpContextAccessor,
            GetPaymentQueryHandler getPaymentQueryHandler,
            CreatePaymentCommandHandler createPaymentCommandHandler,
            UpdatePaymentCommandHandler updatePaymentCommandHandler,
            DeletePaymentCommandHandler deletePaymentCommandHandler,
            IValidator<CreatePaymentCommand> createPaymentCommandValidator,
            IValidator<UpdatePaymentCommand> updatePaymentCommandValidator,
            ILogger<PaymentService> logger)
        {
            _httpContext = httpContextAccessor.HttpContext!;
            _getPaymentQueryHandler = getPaymentQueryHandler;
            _createPaymentCommandHandler = createPaymentCommandHandler;
            _updatePaymentCommandHandler = updatePaymentCommandHandler;
            _deletePaymentCommandHandler = deletePaymentCommandHandler;
            _createPaymentCommandValidator = createPaymentCommandValidator;
            _updatePaymentCommandValidator = updatePaymentCommandValidator;
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
                var (year, month) = filterQuery.GetRequiredYearMonth();

                var query = new GetPaymentQuery(
                    filterQuery.CategoryFilter,
                    filterQuery.DescriptionFilter,
                    filterQuery.CategoryId,
                    filterQuery.MinAmount,
                    filterQuery.MaxAmount,
                    year,
                    month,
                    filterQuery.PageNumber ?? 1,
                    filterQuery.PageSize ?? 20,
                    filterQuery.SortBy,
                    filterQuery.SortOrder);
                var result = await _getPaymentQueryHandler.Handle(query, cancellationToken);

                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid payments filter query");
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments");
                return Results.Problem(
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
                var query = new GetPaymentQuery { Id = id, PageSize = 1 };
                var result = await _getPaymentQueryHandler.Handle(query, cancellationToken);
                var payment = result.Items.FirstOrDefault();

                if (payment == null)
                    return Results.NotFound();

                return Results.Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
                return Results.Problem(
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

                command.CreatedById = GetCurrentUserId();

                // Extract idempotency key from header if present
                var idempotencyKey = _httpContext.Request.Headers["X-Idempotency-Key"].ToString();
                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    command.IdempotencyKey = idempotencyKey;
                }

                await _createPaymentCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

                var id = await _createPaymentCommandHandler.Handle(command, cancellationToken);
                _logger.LogInformation("Payment created successfully with ID: {PaymentId}", id);
                return Results.Created($"/api/v1/payments/{id}", id);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating payment");
                return Results.ValidationProblem(ToValidationErrors(ex));
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
                command.ModifiedById = GetCurrentUserId();

                await _updatePaymentCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

                var result = await _updatePaymentCommandHandler.Handle(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment updated successfully with ID: {PaymentId}", id);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating payment with ID: {PaymentId}", id);
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment with ID: {PaymentId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error updating payment");
            }
        }

        /// <summary>
        /// Deletes a payment by ID.
        /// </summary>
        public async Task<IResult> DeletePaymentAsync(Guid id, string? occurrenceAction, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);

                if (!TryParseOccurrenceAction(occurrenceAction, out var parsedAction))
                    return Results.BadRequest(new { message = "Occurrence action must be Auto, Reopen, or Skip." });

                var command = new DeletePaymentCommand
                {
                    PaymentId = id,
                    DeletedBy = GetCurrentUserId(),
                    OccurrenceAction = parsedAction
                };
                var result = await _deletePaymentCommandHandler.Handle(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment deleted successfully with ID: {PaymentId}", id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting payment");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : SystemUsers.SystemUserId;
        }

        private static bool TryParseOccurrenceAction(string? occurrenceAction, out ForecastOccurrenceDeleteAction action)
        {
            if (string.IsNullOrWhiteSpace(occurrenceAction))
            {
                action = ForecastOccurrenceDeleteAction.Auto;
                return true;
            }

            return Enum.TryParse(occurrenceAction, true, out action);
        }

        private static Dictionary<string, string[]> ToValidationErrors(ValidationException exception)
        {
            return exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }
    }
}
