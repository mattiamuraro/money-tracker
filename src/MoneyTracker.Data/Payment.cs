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
        public Guid PaymentCategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsOneShot { get; set; } = false;

        public PaymentCategory PaymentCategory { get; set; } = null!;
    }
}
