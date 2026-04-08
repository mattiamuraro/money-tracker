using FluentValidation;
using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetCategoryById;

namespace MoneyTracker.Api.Services
{
    /// <summary>
    /// Service for managing payment categories
    /// </summary>
    public class PaymentCategoryService
    {
        private readonly HttpContext _httpContext;
        private readonly GetAllCategoriesQueryHandler _getAllCategoriesQueryHandler;
        private readonly GetCategoryByIdQueryHandler _getCategoryByIdQueryHandler;
        private readonly CreateCategoryCommandHandler _createCategoryCommandHandler;
        private readonly UpdateCategoryCommandHandler _updateCategoryCommandHandler;
        private readonly DeleteCategoryCommandHandler _deleteCategoryCommandHandler;
        private readonly IValidator<CreateCategoryCommand> _createCategoryCommandValidator;
        private readonly IValidator<UpdateCategoryCommand> _updateCategoryCommandValidator;
        private readonly ILogger<PaymentCategoryService> _logger;

        public PaymentCategoryService(
            IHttpContextAccessor httpContextAccessor,
            GetAllCategoriesQueryHandler getAllCategoriesQueryHandler,
            GetCategoryByIdQueryHandler getCategoryByIdQueryHandler,
            CreateCategoryCommandHandler createCategoryCommandHandler,
            UpdateCategoryCommandHandler updateCategoryCommandHandler,
            DeleteCategoryCommandHandler deleteCategoryCommandHandler,
            IValidator<CreateCategoryCommand> createCategoryCommandValidator,
            IValidator<UpdateCategoryCommand> updateCategoryCommandValidator,
            ILogger<PaymentCategoryService> logger)
        {
            _httpContext = httpContextAccessor.HttpContext!;
            _getAllCategoriesQueryHandler = getAllCategoriesQueryHandler;
            _getCategoryByIdQueryHandler = getCategoryByIdQueryHandler;
            _createCategoryCommandHandler = createCategoryCommandHandler;
            _updateCategoryCommandHandler = updateCategoryCommandHandler;
            _deleteCategoryCommandHandler = deleteCategoryCommandHandler;
            _createCategoryCommandValidator = createCategoryCommandValidator;
            _updateCategoryCommandValidator = updateCategoryCommandValidator;
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
                var result = await _getAllCategoriesQueryHandler.Handle(query, cancellationToken);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment categories");
                return Results.Problem(
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
                var result = await _getCategoryByIdQueryHandler.Handle(query, cancellationToken);

                if (result == null)
                    return Results.NotFound();

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment category with ID: {CategoryId}", id);
                return Results.Problem(
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

                await _createCategoryCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

                var categoryId = await _createCategoryCommandHandler.Handle(command, cancellationToken);

                _logger.LogInformation("Payment category created successfully with ID: {CategoryId}", categoryId);
                return Results.Created($"/api/v1/categories/{categoryId}", categoryId);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating category");
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating category");
                return Results.BadRequest(new { message = "A category with this code already exists." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment category");
                return Results.Problem(
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

                await _updateCategoryCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

                var result = await _updateCategoryCommandHandler.Handle(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment category updated successfully with ID: {CategoryId}", id);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating category with ID: {CategoryId}", id);
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating category");
                return Results.BadRequest(new { message = "A category with this code already exists." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment category with ID: {CategoryId}", id);
                return Results.Problem(
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
                var result = await _deleteCategoryCommandHandler.Handle(command, cancellationToken);

                if (!result)
                    return Results.NotFound();

                _logger.LogInformation("Payment category deleted successfully with ID: {CategoryId}", id);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete category with associated payments");
                return Results.BadRequest(new { message = "Cannot delete a category that has associated payments." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment category with ID: {CategoryId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting payment category");
            }
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
