namespace MoneyTracker.Data.Tests;

[TestFixture]
public class ForecastRecurrenceRuleTests
{
    [Test]
    public void GetNextOccurrence_Should_Return_Null_When_DayInterval_Is_Null()
    {
        var rule = new ForecastRecurrenceRule
        {
            Id = Guid.NewGuid(),
            Name = "No Rule",
            Code = "NONE",
            DayInterval = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var result = rule.GetNextOccurrence(new DateOnly(2026, 4, 1));

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetNextOccurrence_Should_Add_DayInterval()
    {
        var rule = new ForecastRecurrenceRule
        {
            Id = Guid.NewGuid(),
            Name = "Weekly",
            Code = "WEEK",
            DayInterval = 7,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var result = rule.GetNextOccurrence(new DateOnly(2026, 4, 1));

        Assert.That(result, Is.EqualTo(new DateOnly(2026, 4, 8)));
    }
}
