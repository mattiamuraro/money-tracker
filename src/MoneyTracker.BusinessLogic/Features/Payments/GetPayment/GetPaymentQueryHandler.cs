using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

public class GetPaymentQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetPaymentQuery, PaginatedResponse<PaymentDto>>
{
    public async Task<PaginatedResponse<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Payments
            .AsNoTracking()
            .AsQueryable();

        if (request.Id.HasValue)
            query = query.Where(p => p.Id == request.Id.Value);
        if (request.Year.HasValue && request.Month.HasValue)
        {
            var monthStart = new DateTime(request.Year.Value, request.Month.Value, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            query = query.Where(p => p.Date >= monthStart && p.Date < nextMonthStart);
        }
        if (!string.IsNullOrWhiteSpace(request.CategoryFilter))
        {
            var normalizedCategoryFilter = request.CategoryFilter.Trim().ToUpperInvariant();
            query = query.Where(p => p.PaymentCategory.NameNormalized.StartsWith(normalizedCategoryFilter));
        }
        if (!string.IsNullOrWhiteSpace(request.DescriptionFilter))
        {
            var normalizedDescriptionFilter = request.DescriptionFilter.Trim().ToUpperInvariant();
            query = query.Where(p => p.DescriptionNormalized.StartsWith(normalizedDescriptionFilter));
        }
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.PaymentCategoryId == request.CategoryId.Value);
        if (request.MinAmount.HasValue)
            query = query.Where(p => p.Amount >= request.MinAmount.Value);
        if (request.MaxAmount.HasValue)
            query = query.Where(p => p.Amount <= request.MaxAmount.Value);

        var totalItems = await query.CountAsync(cancellationToken);
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var payments = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Description = p.Description,
                PaymentCategoryId = p.PaymentCategoryId,
                Category = p.PaymentCategory.Name,
                ForecastOccurrenceId = p.ForecastOccurrenceId,
                ForecastExpectedDate = p.ForecastOccurrence != null ? p.ForecastOccurrence.ExpectedDate : null,
                Amount = p.Amount,
                Date = p.Date,
                IsOneShot = p.IsOneShot
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PaymentDto>
        {
            Items = payments,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems
        };
    }

    private static IQueryable<Payment> ApplySorting(IQueryable<Payment> query, string? sortBy, string? sortOrder)
    {
        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "amount" => isDescending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
            "description" => isDescending ? query.OrderByDescending(p => p.Description) : query.OrderBy(p => p.Description),
            "category" => isDescending ? query.OrderByDescending(p => p.PaymentCategory.Name) : query.OrderBy(p => p.PaymentCategory.Name),
            _ => isDescending ? query.OrderByDescending(p => p.Date) : query.OrderBy(p => p.Date),
        };
    }
}
