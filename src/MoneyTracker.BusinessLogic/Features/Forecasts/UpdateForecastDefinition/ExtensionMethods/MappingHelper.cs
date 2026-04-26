using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static ForecastOccurrence ToNewForecastOccurrence(this OccurrenceSeed entity)
        {
            return new ForecastOccurrence
            {
                Id = Guid.NewGuid(),
                ForecastDefinitionId = entity.ForecastDefinitionId,
                IsIncome = entity.IsIncome,
                Description = entity.Description,
                Amount = entity.Amount,
                ExpectedDate = entity.ExpectedDate,
                PaymentCategoryId = entity.PaymentCategoryId,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            };
        }

        public static void ApplyIncomeForecastDefinitionEdit(this ForecastIncome forecast, UpdateForecastDefinitionCommand command)
        {
            forecast.ApplyForecastDefinitionEdit(command);
        }

        public static void ApplyExpenseForecastDefinitionEdit(this ForecastExpense forecast, UpdateForecastDefinitionCommand command)
        {
            forecast.ApplyForecastDefinitionEdit(command);
            forecast.PaymentCategoryId = command.PaymentCategoryId ?? Guid.Empty;
        }

        private static void ApplyForecastDefinitionEdit(this BaseForecast forecast, UpdateForecastDefinitionCommand command)
        {
            forecast.Description = command.Description;
            forecast.Amount = command.Amount;
            forecast.RecurrenceStart = command.RecurrenceStart;
            forecast.RecurrenceEnd = command.RecurrenceEnd;
            forecast.Interval = command.Interval;
            forecast.ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId;
            forecast.IsActive = true;
        }

        public static OccurrenceSeed ToOccurrenceSeed(this ForecastExpense forecast, DateOnly recurrence)
        {
            return new OccurrenceSeed(
                forecast.Id,
                false,
                forecast.Description,
                forecast.Amount,
                recurrence,
                forecast.PaymentCategoryId);
        }

        public static OccurrenceSeed ToOccurrenceSeed(this ForecastIncome forecast, DateOnly recurrence)
        {
            return new OccurrenceSeed(
                forecast.Id,
                true,
                forecast.Description,
                forecast.Amount,
                recurrence,
                null);
        }
    }
}
