using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetCategoryById;
using MediatR;

namespace MoneyTracker.Api.Services
{
    /// <summary>
    /// Service for managing payment categories
    /// </summary>
    public class PaymentCategoryService
    {
        private readonly HttpContext _httpContext;
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentCategoryService> _logger;

        public PaymentCategoryService(IHttpContextAccessor httpContextAccessor, IMediator mediator, ILogger<PaymentCategoryService> logger)
        {
            _httpContext = httpContextAccessor.HttpContext!;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all payment categories
        /// </summary>
        public async Task<IResult> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all payment categories");
                var query = new GetAllCategoriesQuery();
                var result = await _mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment categories");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving payment categories");
            }
        }

        /// <summary>
        /// Retrieves a specific payment category by ID
        /// </summary>
        public async Task<IResult> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payment category with ID: {CategoryId}", id);
                var query = new GetCategoryByIdQuery(id);
                var result = await _mediator.Send(query, cancellationToken);

                if (result == null)
                    return Results.NotFound();

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment category with ID: {CategoryId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving payment category");
            }
        }

        /// <summary>
        /// Creates a new payment category
        /// </summary>
        public async Task<IResult> CreateCategoryAsync(CreatePaymentCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new payment category: {CategoryCode}", request.Code);

                var command = new CreateCategoryCommand
                {
                    Name = request.Name,
                    Code = request.Code,
                    CreatedBy = _httpContext.User?.FindFirst("sub")?.Value ?? "System"
                };

                var categoryId = await _mediator.Send(command, cancellationToken);

                _logger.LogInformation("Payment category created successfully with ID: {CategoryId}", categoryId);
                return Results.Created($"/api/v1/categories/{categoryId}", categoryId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating category");
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment category");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error creating payment category");
            }
        }

        /// <summary>
        /// Updates an existing payment category
        /// </summary>
        public async Task<IResult> UpdateCategoryAsync(Guid id, UpdatePaymentCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating payment category with ID: {CategoryId}", id);

                var command = new UpdateCategoryCommand
                {
                    Id = id,
                    Name = request.Name,
                    Code = request.Code,
                    ModifiedBy = _httpContext.User?.FindFirst("sub")?.Value ?? "System"
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment category updated successfully with ID: {CategoryId}", id);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating category");
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment category with ID: {CategoryId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error updating payment category");
            }
        }

        /// <summary>
        /// Deletes a payment category
        /// </summary>
        public async Task<IResult> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting payment category with ID: {CategoryId}", id);

                var command = new DeleteCategoryCommand(id);
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment category deleted successfully with ID: {CategoryId}", id);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete category with associated payments");
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment category with ID: {CategoryId}", id);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting payment category");
            }
        }
    }
}
