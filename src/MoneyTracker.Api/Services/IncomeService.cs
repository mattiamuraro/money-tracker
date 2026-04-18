using FluentValidation;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.Data;
using System.Security.Claims;

namespace MoneyTracker.Api.Services;

public class IncomeService
{
    private readonly HttpContext _httpContext;
    private readonly GetIncomeQueryHandler _getIncomeQueryHandler;
    private readonly CreateIncomeCommandHandler _createIncomeCommandHandler;
    private readonly UpdateIncomeCommandHandler _updateIncomeCommandHandler;
    private readonly DeleteIncomeCommandHandler _deleteIncomeCommandHandler;
    private readonly IValidator<CreateIncomeCommand> _createIncomeCommandValidator;
    private readonly IValidator<UpdateIncomeCommand> _updateIncomeCommandValidator;
    private readonly ILogger<IncomeService> _logger;

    public IncomeService(
        IHttpContextAccessor httpContextAccessor,
        GetIncomeQueryHandler getIncomeQueryHandler,
        CreateIncomeCommandHandler createIncomeCommandHandler,
        UpdateIncomeCommandHandler updateIncomeCommandHandler,
        DeleteIncomeCommandHandler deleteIncomeCommandHandler,
        IValidator<CreateIncomeCommand> createIncomeCommandValidator,
        IValidator<UpdateIncomeCommand> updateIncomeCommandValidator,
        ILogger<IncomeService> logger)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _getIncomeQueryHandler = getIncomeQueryHandler;
        _createIncomeCommandHandler = createIncomeCommandHandler;
        _updateIncomeCommandHandler = updateIncomeCommandHandler;
        _deleteIncomeCommandHandler = deleteIncomeCommandHandler;
        _createIncomeCommandValidator = createIncomeCommandValidator;
        _updateIncomeCommandValidator = updateIncomeCommandValidator;
        _logger = logger;
    }

    public async Task<IResult> GetIncomesAsync(IncomeFilterQuery filterQuery, CancellationToken cancellationToken)
    {
        try
        {
            filterQuery.Validate();
            var (year, month) = filterQuery.GetRequiredYearMonth();

            var query = new GetIncomeQuery(
                filterQuery.DescriptionFilter,
                filterQuery.MinAmount,
                filterQuery.MaxAmount,
                year,
                month,
                filterQuery.PageNumber ?? 1,
                filterQuery.PageSize ?? 20,
                filterQuery.SortBy,
                filterQuery.SortOrder);

            var result = await _getIncomeQueryHandler.Handle(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incomes");
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error retrieving incomes");
        }
    }

    public async Task<IResult> GetIncomeByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetIncomeQuery { Id = id, PageSize = 1 };
            var result = await _getIncomeQueryHandler.Handle(query, cancellationToken);
            var income = result.Items.FirstOrDefault();

            return income is null ? Results.NotFound() : Results.Ok(income);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving income with ID: {IncomeId}", id);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error retrieving income");
        }
    }

    public async Task<IResult> CreateIncomeAsync(CreateIncomeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = _httpContext.Request.Headers["X-Idempotency-Key"].ToString();
            var command = new CreateIncomeCommand
            {
                Description = request.Description,
                ForecastOccurrenceId = request.ForecastOccurrenceId,
                Amount = request.Amount,
                Date = request.Date,
                CreatedById = GetCurrentUserId(),
                IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? request.IdempotencyKey : idempotencyKey
            };

            await _createIncomeCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

            var id = await _createIncomeCommandHandler.Handle(command, cancellationToken);
            return Results.Created($"/api/v1/incomes/{id}", id);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(ToValidationErrors(ex));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating income");
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error creating income");
        }
    }

    public async Task<IResult> UpdateIncomeAsync(Guid id, UpdateIncomeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateIncomeCommand
            {
                IncomeId = id,
                Description = request.Description,
                Amount = request.Amount,
                Date = request.Date,
                ModifiedById = GetCurrentUserId()
            };

            await _updateIncomeCommandValidator.ValidateAndThrowAsync(command, cancellationToken);

            var updated = await _updateIncomeCommandHandler.Handle(command, cancellationToken);

            return updated ? Results.NoContent() : Results.NotFound();
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(ToValidationErrors(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating income with ID: {IncomeId}", id);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error updating income");
        }
    }

    public async Task<IResult> DeleteIncomeAsync(Guid id, string? occurrenceAction, CancellationToken cancellationToken)
    {
        try
        {
            if (!TryParseOccurrenceAction(occurrenceAction, out var parsedAction))
                return Results.BadRequest(new { message = "Occurrence action must be Auto, Reopen, or Skip." });

            var command = new DeleteIncomeCommand
            {
                IncomeId = id,
                DeletedBy = GetCurrentUserId(),
                OccurrenceAction = parsedAction
            };

            var deleted = await _deleteIncomeCommandHandler.Handle(command, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting income with ID: {IncomeId}", id);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error deleting income");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : SystemUsers.SystemUserId;
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
