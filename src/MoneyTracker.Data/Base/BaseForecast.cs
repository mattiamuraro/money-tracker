using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MoneyTracker.Data.Base
{
    public class BaseForecast : BaseEntity
    {
        [MaxLength(100)]
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly RecurrenceStart { get; set; }
        public DateOnly? RecurrenceEnd { get; set; }
        public Guid ForecastRecurrenceRuleId { get; set; }

        public ForecastRecurrenceRule ForecastRecurrenceRule { get; set; } = null!;

        public List<DateOnly> GetRecurrences(DateOnly startDate, DateOnly endDate)
        {
            var occurrencyDate = this.RecurrenceStart;
            var result = new List<DateOnly>();

            while (occurrencyDate <= endDate)
            {
                if (occurrencyDate >= startDate)
                    result.Add(occurrencyDate);

                if (this.ForecastRecurrenceRule.GetNextOccurrence(occurrencyDate) is DateOnly nextOccurrency)
                    occurrencyDate = nextOccurrency;
                else
                    break;
            }

            return result;
        }
    }
}