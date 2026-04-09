using MoneyTracker.Data.Base;

namespace MoneyTracker.Data
{
    public class ForecastRecurrenceRuleType : BaseContextEntity
    {
        public const string OneTime = "O";
        public const string Day = "D";
        public const string Week = "W";
        public const string Month = "M";
        public const string Year = "Y";
    }
}
