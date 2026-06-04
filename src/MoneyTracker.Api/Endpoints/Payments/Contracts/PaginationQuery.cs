namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

/// <summary>
/// Standard pagination query parameters
/// </summary>
public class PaginationQuery
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int? PageNumber { get; set; } = DefaultPageNumber;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int? PageSize { get; set; } = DefaultPageSize;

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
        PageNumber = GetPageNumber();
        PageSize = GetPageSize();

        if (!new[] { "asc", "desc" }.Contains(SortOrder?.ToLowerInvariant()))
            SortOrder = "desc";
    }

    public int GetPageNumber()
        => Math.Max(DefaultPageNumber, PageNumber ?? DefaultPageNumber);

    public int GetPageSize()
        => Math.Clamp(PageSize ?? DefaultPageSize, 1, MaxPageSize);
}
