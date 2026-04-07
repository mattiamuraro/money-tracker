using MediatR;
using MoneyTracker.BusinessLogic.Features.Payments.Models;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;

/// <summary>
/// Query to retrieve payment history with pagination and filters
/// </summary>
public class GetPaymentQuery : IRequest<PaginatedResponse<PaymentRow>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CategoryFilter { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "desc";

    public GetPaymentQuery() { }

    public GetPaymentQuery(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? categoryFilter = null,
        Guid? categoryId = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "desc")
    {
        StartDate = startDate;
        EndDate = endDate;
        CategoryFilter = categoryFilter;
        CategoryId = categoryId;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Clamp(pageSize, 1, 100);
        SortBy = sortBy;
        SortOrder = sortOrder ?? "desc";
    }
}
