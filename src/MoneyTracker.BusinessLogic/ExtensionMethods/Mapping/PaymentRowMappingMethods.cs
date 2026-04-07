using MoneyTracker.BusinessLogic.Features.Payments.Models;
using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.ExtensionMethods.Mapping
{
    public static class PaymentRowMappingMethods
    {
        public static List<PaymentRow> ToPaymentRows(this IEnumerable<Payment> payments)
        {
            return payments.Select(p => p.ToPaymentRow()).ToList();
        }

        public static PaymentRow ToPaymentRow(this Payment payment)
        {
            return new PaymentRow
            {
                Id = payment.Id,
                Description = payment.Description,
                PaymentCategoryId = payment.PaymentCategoryId,
                Category = payment.PaymentCategory.Name,
                Amount = payment.Amount,
                Date = payment.Date,
                IsOneShot = payment.IsOneShot
            };
        }
    }
}
