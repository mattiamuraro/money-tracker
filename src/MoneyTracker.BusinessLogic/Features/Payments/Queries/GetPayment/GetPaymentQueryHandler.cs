using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Features.Payments.Models;
using MoneyTracker.BusinessLogic.Shared.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;

public class GetPaymentQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetPaymentQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<PaymentRow>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Payments.Include(p => p.PaymentCategory).AsQueryable();

        if (request.Id.HasValue)
            query = query.Where(p => p.Id == request.Id.Value);

        if (request.Year.HasValue && request.Month.HasValue)
            query = query.Where(p => p.Date.Year == request.Year.Value && p.Date.Month == request.Month.Value);

        if (request.StartDate.HasValue)
            query = query.Where(p => p.Date >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.Date <= request.EndDate.Value);

        if (!string.IsNullOrEmpty(request.CategoryFilter))
            query = query.Where(p => p.PaymentCategory.Name.Contains(request.CategoryFilter));

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
            .Select(p => new PaymentRow
            {
                Id = p.Id,
                Description = p.Description,
                PaymentCategoryId = p.PaymentCategoryId,
                Category = p.PaymentCategory.Name,
                Amount = p.Amount,
                Date = p.Date,
                IsOneShot = p.IsOneShot
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PaymentRow>
        {
            Items = payments,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<Payment> ApplySorting(
        IQueryable<Payment> query,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return (sortBy?.ToLower()) switch
        {
            "amount" => isDescending
                ? query.OrderByDescending(p => p.Amount)
                : query.OrderBy(p => p.Amount),
            "description" => isDescending
                ? query.OrderByDescending(p => p.Description)
                : query.OrderBy(p => p.Description),
            "category" => isDescending
                ? query.OrderByDescending(p => p.PaymentCategory.Name)
                : query.OrderBy(p => p.PaymentCategory.Name),
            _ => isDescending
                ? query.OrderByDescending(p => p.Date)
                : query.OrderBy(p => p.Date),
        };
    }
}
