using MoneyTracker.Data;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data.Base
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid CreatedById { get; set; }

        public User CreatedBy { get; set; } = default!;

        public DateTime ModifiedAt { get; set; }

        public Guid ModifiedById { get; set; }

        public User ModifiedBy { get; set; } = default!;
    }
}
