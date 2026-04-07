using MoneyTracker.Data.Base;

namespace MoneyTracker.Data
{
    public class PaymentCategory : BaseContextEntity
    {
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
