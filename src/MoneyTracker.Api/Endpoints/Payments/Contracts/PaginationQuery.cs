namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

/// <summary>
/// Standard pagination query parameters
/// </summary>
public class PaginationQuery
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int? PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int? PageSize { get; set; } = 20;

    /// <summary>
    /// Field to sort by
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order: 'asc' or 'desc'
    /// </summary>
    public string? SortOrder { get; set; } = "desc";

    /// <summary>
    /// Validate pagination parameters
    /// </summary>
    public void Validate()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 1;
        else if (PageSize > 100)
            PageSize = 100;

        if (!new[] { "asc", "desc" }.Contains(SortOrder?.ToLower()))
            SortOrder = "desc";
    }
}
