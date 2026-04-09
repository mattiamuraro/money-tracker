namespace MoneyTracker.Data.Tests;

[TestFixture]
public class ForecastRecurrenceRuleTests
{
    [Test]
    public void GetRecurrences_Should_Return_Only_Start_Date_When_Interval_Is_Null()
    {
        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = Guid.NewGuid(),
            Name = "Day",
            Code = ForecastRecurrenceRuleType.Day,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var forecast = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "No interval",
            Amount = 10m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            Interval = null,
            ForecastRecurrenceRuleTypeId = ruleType.Id,
            ForecastRecurrenceRuleType = ruleType,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var result = forecast.GetRecurrences(new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30));

        Assert.That(result, Is.EqualTo(new List<DateOnly> { new(2026, 4, 1) }));
    }

    [Test]
    public void GetRecurrences_Should_Add_Interval_When_Type_Is_Day()
    {
        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = Guid.NewGuid(),
            Name = "Day",
            Code = ForecastRecurrenceRuleType.Day,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var forecast = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Daily",
            Amount = 10m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            Interval = 7,
            ForecastRecurrenceRuleTypeId = ruleType.Id,
            ForecastRecurrenceRuleType = ruleType,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var result = forecast.GetRecurrences(new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 20));

        Assert.That(result, Is.EqualTo(new List<DateOnly>
        {
            new(2026, 4, 1),
            new(2026, 4, 8),
            new(2026, 4, 15)
        }));
    }
}
