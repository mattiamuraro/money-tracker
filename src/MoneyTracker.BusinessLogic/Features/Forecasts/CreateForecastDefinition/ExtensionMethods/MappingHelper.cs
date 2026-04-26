using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static ForecastOccurrence ToNewForecastOccurrence(this ForecastIncome entity, DateOnly recurrence)
        {
            return entity.ToNewForecastOccurrence(recurrence, Guid.NewGuid());
        }

        public static ForecastOccurrence ToNewForecastOccurrence(this ForecastIncome entity, DateOnly recurrence, Guid id)
        {
            return new ForecastOccurrence
            {
                Id = id,
                ForecastDefinitionId = entity.Id,
                IsIncome = true,
                Description = entity.Description,
                Amount = entity.Amount,
                ExpectedDate = recurrence,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            };
        }

        public static ForecastOccurrence ToNewForecastOccurrence(this ForecastExpense entity, DateOnly recurrence)
        {
            return entity.ToNewForecastOccurrence(recurrence, Guid.NewGuid());
        }

        public static ForecastOccurrence ToNewForecastOccurrence(this ForecastExpense entity, DateOnly recurrence, Guid id)
        {
            return new ForecastOccurrence
            {
                Id = id,
                ForecastDefinitionId = entity.Id,
                IsIncome = false,
                Description = entity.Description,
                Amount = entity.Amount,
                ExpectedDate = recurrence,
                PaymentCategoryId = entity.PaymentCategoryId,
                ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
            };
        }

        public static ForecastIncome ToNewForecastIncome(this CreateForecastDefinitionCommand command)
        {
            return new ForecastIncome
            {
                Id = Guid.NewGuid(),
                Description = command.Description,
                Amount = command.Amount,
                RecurrenceStart = command.RecurrenceStart,
                RecurrenceEnd = command.RecurrenceEnd,
                Interval = command.Interval,
                ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId,
                IsActive = true
            };
        }

        public static ForecastExpense ToNewForecastExpense(this CreateForecastDefinitionCommand command)
        {
            return new ForecastExpense
            {
                Id = Guid.NewGuid(),
                PaymentCategoryId = command.PaymentCategoryId!.Value,
                Description = command.Description,
                Amount = command.Amount,
                RecurrenceStart = command.RecurrenceStart,
                RecurrenceEnd = command.RecurrenceEnd,
                Interval = command.Interval,
                ForecastRecurrenceRuleTypeId = command.ForecastRecurrenceRuleTypeId,
                IsActive = true
            };
        }
    }
}
