using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.ExtensionMethods
{
    internal static class DeletePaymentHelpers
    {
        public static bool TryParseOccurrenceAction(this string? occurrenceAction, out ForecastOccurrenceDeleteAction action)
        {
            if (string.IsNullOrWhiteSpace(occurrenceAction))
            {
                action = ForecastOccurrenceDeleteAction.Auto;
                return true;
            }

            return Enum.TryParse(occurrenceAction, true, out action);
        }
    }
}
