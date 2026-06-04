using MoneyTracker.Api.ExtensionMethods;

namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

/// <summary>
/// Filter parameters for payment queries
/// </summary>
public class PaymentFilterQuery : PaginationQuery
{
    private const string MonthValidationMessage = "Month filter is required and must use yyyy-MM format.";

    /// <summary>
    /// Required month filter in yyyy-MM format
    /// </summary>
    public string? Month { get; set; }

    /// <summary>
    /// Filter by category name
    /// </summary>
    public string? CategoryFilter { get; set; }

    /// <summary>
    /// Filter by payment description text
    /// </summary>
    public string? DescriptionFilter { get; set; }

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

    public (int Year, int Month) GetRequiredYearMonth()
        => Month.GetRequiredYearMonth(MonthValidationMessage);
}
