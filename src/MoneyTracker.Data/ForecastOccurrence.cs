using MoneyTracker.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data;

public class ForecastOccurrence : BaseEntity
{
    public Guid ForecastDefinitionId { get; set; }
    public bool IsIncome { get; set; }

    [MaxLength(100)]
    public required string Description { get; set; }

    public decimal Amount { get; set; }
    public DateOnly ExpectedDate { get; set; }
    public Guid? PaymentCategoryId { get; set; }
    public Guid ForecastOccurrenceStatusId { get; set; } = ForecastOccurrenceStatus.PendingId;
    public DateTime? ValidatedAt { get; set; }

    public PaymentCategory? PaymentCategory { get; set; }
    public ForecastOccurrenceStatus Status { get; set; } = null!;
}
