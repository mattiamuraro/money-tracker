namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

/// <summary>
/// Filter parameters for payment queries
/// </summary>
public class PaymentFilterQuery : PaginationQuery
{
    /// <summary>
    /// Filter by start date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter by end date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by category name
    /// </summary>
    public string? CategoryFilter { get; set; }

    /// <summary>
    /// Filter by minimum amount
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Filter by maximum amount
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }
}
