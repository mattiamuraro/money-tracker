namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;

public class GetIncomeQuery
{
    public Guid? Id { get; set; }
    public string? DescriptionFilter { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "desc";

    public GetIncomeQuery() { }

    public GetIncomeQuery(
        string? descriptionFilter = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int? year = null,
        int? month = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "desc")
    {
        DescriptionFilter = descriptionFilter;
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
