using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data.Base
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime ModifiedAt { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }
    }
}
