namespace MoneyTracker.BusinessLogic.Features.Payments.Queries.GetPaymentHistory;

/// <summary>
/// Query to retrieve payment history with pagination and filters
/// </summary>
public class GetPaymentQuery
{
    /// <summary>
    /// When set, returns only the payment with this ID.
    /// </summary>
    public Guid? Id { get; set; }
    public string? CategoryFilter { get; set; }
    public string? DescriptionFilter { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "desc";

    public GetPaymentQuery() { }

    public GetPaymentQuery(
        string? categoryFilter = null,
        string? descriptionFilter = null,
        Guid? categoryId = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int? year = null,
        int? month = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "desc")
    {
        CategoryFilter = categoryFilter;
        DescriptionFilter = descriptionFilter;
        CategoryId = categoryId;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Year = year;
        Month = month;
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Clamp(pageSize, 1, 100);
        SortBy = sortBy;
        SortOrder = sortOrder ?? "desc";
    }
}
