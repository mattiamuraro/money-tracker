using MoneyTracker.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data
{
    public class Income : SoftDeleteEntity
    {
        [MaxLength(100)]
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        [MaxLength(256)]
        public string? IdempotencyKey { get; set; }
    }
}
