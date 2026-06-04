using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;

public class UpdateForecastExpenseRequest
{
    public Guid ForecastRecurrenceRuleTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; set; }

    public DateOnly RecurrenceStart { get; set; }

    public DateOnly? RecurrenceEnd { get; set; }

    [Range(1, 365)]
    public int Interval { get; set; }

    public Guid? PaymentCategoryId { get; set; }
}
