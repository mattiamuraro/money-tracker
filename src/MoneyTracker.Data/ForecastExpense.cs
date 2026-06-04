using MoneyTracker.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data
{
    public class ForecastExpense : BaseForecast
    {
        public Guid PaymentCategoryId { get; set; }
        public PaymentCategory PaymentCategory { get; set; } = null!;
    }
}