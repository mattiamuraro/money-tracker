using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data.Base
{
    public class BaseForecast : BaseEntity
    {
        [MaxLength(100)]
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly RecurrenceStart { get; set; }
        public DateOnly? RecurrenceEnd { get; set; }

        public int? Interval { get; set; }
        public Guid ForecastRecurrenceRuleTypeId { get; set; }

        public ForecastRecurrenceRuleType ForecastRecurrenceRuleType { get; set; } = null!;

        public List<DateOnly> GetRecurrences(DateOnly startDate, DateOnly endDate)
        {
            var occurrencyDate = this.RecurrenceStart;
            var result = new List<DateOnly>();

            while (occurrencyDate <= endDate && (RecurrenceEnd == null || occurrencyDate <= RecurrenceEnd))
            {
                if (occurrencyDate >= startDate)
                    result.Add(occurrencyDate);

                if (this.GetNextOccurrence(occurrencyDate) is DateOnly nextOccurrency)
                    occurrencyDate = nextOccurrency;
                else
                    break;
            }

            return result;
        }
        private DateOnly? GetNextOccurrence(DateOnly currentDate)
        {
            if (Interval is int value)
            {
                switch (ForecastRecurrenceRuleType.Code)
                {
                    case ForecastRecurrenceRuleType.OneTime:
                        return null;
                    case ForecastRecurrenceRuleType.Day:
                        return currentDate.AddDays(value);
                    case ForecastRecurrenceRuleType.Week:
                        return currentDate.AddDays(7 * value);
                    case ForecastRecurrenceRuleType.Month:
                        return currentDate.AddMonths(value);
                    case ForecastRecurrenceRuleType.Year:
                        return currentDate.AddYears(value);
                }
            }

            return null;
        }
    }
}