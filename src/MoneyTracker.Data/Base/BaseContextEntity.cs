using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data.Base
{
    public class BaseContextEntity : BaseEntity
    {
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(5)]
        public string Code { get; set; }

        public int? OrderIndex { get; set; } = default(int?);
    }
}
