using MoneyTracker.Data.Base;

namespace MoneyTracker.Data;

public class ForecastOccurrenceStatus : BaseContextEntity
{
    public static readonly Guid PendingId = Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid ConfirmedId = Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid SkippedId = Guid.Parse("00000000-0000-0000-0000-000000000103");
    public static readonly Guid CancelledId = Guid.Parse("00000000-0000-0000-0000-000000000104");

    public const string Pending = "PEN";
    public const string Confirmed = "CNF";
    public const string Skipped = "SKP";
    public const string Cancelled = "CNL";
}
