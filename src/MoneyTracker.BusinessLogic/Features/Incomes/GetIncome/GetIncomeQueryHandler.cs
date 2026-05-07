using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;

public class GetIncomeQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetIncomeQuery, PaginatedResponse<IncomeRow>>
{
    public async Task<PaginatedResponse<IncomeRow>> Handle(GetIncomeQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Incomes
            .AsNoTracking()
            .AsQueryable();

        if (request.Id.HasValue)
            query = query.Where(x => x.Id == request.Id.Value);
        if (request.Year.HasValue && request.Month.HasValue)
            query = query.Where(x => x.Date.Year == request.Year.Value && x.Date.Month == request.Month.Value);
        if (!string.IsNullOrWhiteSpace(request.DescriptionFilter))
            query = query.Where(x => x.Description.Contains(request.DescriptionFilter));
        if (request.MinAmount.HasValue)
            query = query.Where(x => x.Amount >= request.MinAmount.Value);
        if (request.MaxAmount.HasValue)
            query = query.Where(x => x.Amount <= request.MaxAmount.Value);

        var totalItems = await query.CountAsync(cancellationToken);
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new IncomeRow
            {
                Id = x.Id,
                Description = x.Description,
                ForecastOccurrenceId = x.ForecastOccurrenceId,
                ForecastExpectedDate = x.ForecastOccurrence != null ? x.ForecastOccurrence.ExpectedDate : null,
                Amount = x.Amount,
                Date = x.Date,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<IncomeRow>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    private static IQueryable<Income> ApplySorting(IQueryable<Income> query, string? sortBy, string? sortOrder)
    {
        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "amount" => isDescending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "description" => isDescending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
            _ => isDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
        };
    }
}
