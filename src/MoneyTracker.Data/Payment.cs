using MoneyTracker.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data
{
    /// <summary>
    /// Payment entity with soft delete support
    /// </summary>
    public class Payment : SoftDeleteEntity
    {
        [MaxLength(100)]
        public required string Description { get; set; }

        [MaxLength(100)]
        public string DescriptionNormalized { get; set; } = string.Empty;
        public Guid PaymentCategoryId { get; set; }
        public Guid? ForecastOccurrenceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsOneShot { get; set; } = false;

        /// <summary>
        /// Optional client-supplied key used to prevent duplicate payment creation on retries.
        /// </summary>
        [MaxLength(256)]
        public string? IdempotencyKey { get; set; }

        public PaymentCategory PaymentCategory { get; set; } = null!;
        public ForecastOccurrence? ForecastOccurrence { get; set; }
    }
}
