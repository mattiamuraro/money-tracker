using Microsoft.EntityFrameworkCore;
using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.BusinessLogic.Shared.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.Security.Claims;

namespace MoneyTracker.Api.Services;

public class IncomeService
{
    private readonly HttpContext _httpContext;
    private readonly MoneyTrackerDbContext _dbContext;
    private readonly ILogger<IncomeService> _logger;

    public IncomeService(
        IHttpContextAccessor httpContextAccessor,
        MoneyTrackerDbContext dbContext,
        ILogger<IncomeService> logger)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IResult> GetIncomesAsync(IncomeFilterQuery filterQuery, CancellationToken cancellationToken)
    {
        try
        {
            filterQuery.Validate();
            var (year, month) = filterQuery.GetRequiredYearMonth();

            var query = _dbContext.Incomes.AsQueryable();

            query = query.Where(x => x.Date.Year == year && x.Date.Month == month);

            if (!string.IsNullOrWhiteSpace(filterQuery.DescriptionFilter))
                query = query.Where(x => x.Description.Contains(filterQuery.DescriptionFilter));

            if (filterQuery.MinAmount.HasValue)
                query = query.Where(x => x.Amount >= filterQuery.MinAmount.Value);

            if (filterQuery.MaxAmount.HasValue)
                query = query.Where(x => x.Amount <= filterQuery.MaxAmount.Value);

            query = ApplySorting(query, filterQuery.SortBy, filterQuery.SortOrder);

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(((filterQuery.PageNumber ?? 1) - 1) * (filterQuery.PageSize ?? 20))
                .Take(filterQuery.PageSize ?? 20)
                .Select(x => new IncomeRow
                {
                    Id = x.Id,
                    Description = x.Description,
                    Amount = x.Amount,
                    Date = x.Date,
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(new PaginatedResponse<IncomeRow>
            {
                Items = items,
                PageNumber = filterQuery.PageNumber ?? 1,
                PageSize = filterQuery.PageSize ?? 20,
                TotalItems = totalItems,
            });
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
            var income = await _dbContext.Incomes
                .Where(x => x.Id == id)
                .Select(x => new IncomeRow
                {
                    Id = x.Id,
                    Description = x.Description,
                    Amount = x.Amount,
                    Date = x.Date,
                })
                .FirstOrDefaultAsync(cancellationToken);

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
        if (string.IsNullOrWhiteSpace(request.Description))
            return Results.BadRequest(new { message = "Description is required." });

        if (request.Description.Length > 100)
            return Results.BadRequest(new { message = "Description must not exceed 100 characters." });

        if (request.Amount <= 0)
            return Results.BadRequest(new { message = "Amount must be greater than 0." });

        if (request.Date > DateTime.UtcNow)
            return Results.BadRequest(new { message = "Date cannot be in the future." });

        try
        {
            var idempotencyKey = _httpContext.Request.Headers["X-Idempotency-Key"].ToString();
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await _dbContext.Incomes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

                if (existing is not null)
                    return Results.Created($"/api/v1/incomes/{existing.Id}", existing.Id);
            }

            var income = new Income
            {
                Id = Guid.NewGuid(),
                Description = request.Description.Trim(),
                Amount = request.Amount,
                Date = request.Date,
                IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? request.IdempotencyKey : idempotencyKey,
                CreatedById = GetCurrentUserId(),
                ModifiedById = GetCurrentUserId(),
            };

            _dbContext.Incomes.Add(income);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/incomes/{income.Id}", income.Id);
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
        if (request.Description is { Length: > 100 })
            return Results.BadRequest(new { message = "Description must not exceed 100 characters." });

        if (request.Amount.HasValue && request.Amount.Value <= 0)
            return Results.BadRequest(new { message = "Amount must be greater than 0." });

        if (request.Date.HasValue && request.Date.Value > DateTime.UtcNow)
            return Results.BadRequest(new { message = "Date cannot be in the future." });

        try
        {
            var income = await _dbContext.Incomes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (income is null)
                return Results.NotFound();

            if (!string.IsNullOrWhiteSpace(request.Description))
                income.Description = request.Description.Trim();

            if (request.Amount.HasValue)
                income.Amount = request.Amount.Value;

            if (request.Date.HasValue)
                income.Date = request.Date.Value;

            income.ModifiedById = GetCurrentUserId();
            income.ModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating income with ID: {IncomeId}", id);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error updating income");
        }
    }

    public async Task<IResult> DeleteIncomeAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var income = await _dbContext.Incomes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (income is null)
                return Results.NotFound();

            income.Delete(GetCurrentUserId());
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
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

    private static IQueryable<Income> ApplySorting(IQueryable<Income> query, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLowerInvariant() == "desc";

        return (sortBy?.ToLowerInvariant()) switch
        {
            "amount" => isDescending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "description" => isDescending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
            _ => isDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
        };
    }
}
