using MoneyTracker.Data.Base;

namespace MoneyTracker.Data
{
    public class PaymentCategory : BaseContextEntity
    {
        public string NameNormalized { get; set; } = string.Empty;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
