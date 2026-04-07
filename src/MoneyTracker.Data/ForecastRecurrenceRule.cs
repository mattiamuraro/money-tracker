using MoneyTracker.Data.Base;

namespace MoneyTracker.Data
{
    public class ForecastRecurrenceRule : BaseContextEntity
    {
        public int? DayInterval { get; set; }

        public DateOnly? GetNextOccurrence(DateOnly currentDate)
        {
            if (DayInterval is int value)
                return currentDate.AddDays(value);

            return null;

        }
    }
}
