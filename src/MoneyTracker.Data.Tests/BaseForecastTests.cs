namespace MoneyTracker.Data.Tests;

[TestFixture]
public class BaseForecastTests
{
    [Test]
    public void GetRecurrences_Should_Return_Expected_Dates_In_Range()
    {
        var forecast = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Subscription",
            Amount = 10m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            ForecastRecurrenceRuleId = Guid.NewGuid(),
            ForecastRecurrenceRule = new ForecastRecurrenceRule
            {
                Id = Guid.NewGuid(),
                Name = "Weekly",
                Code = "WEEK",
                DayInterval = 7,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "tester"
            },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var recurrences = forecast.GetRecurrences(new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 25));

        Assert.That(recurrences, Is.EqualTo(new List<DateOnly>
        {
            new(2026, 4, 8),
            new(2026, 4, 15),
            new(2026, 4, 22)
        }));
    }

    [Test]
    public void GetRecurrences_Should_Stop_When_No_Next_Occurrence()
    {
        var forecast = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "One time",
            Amount = 100m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            ForecastRecurrenceRuleId = Guid.NewGuid(),
            ForecastRecurrenceRule = new ForecastRecurrenceRule
            {
                Id = Guid.NewGuid(),
                Name = "None",
                Code = "NONE",
                DayInterval = null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "tester"
            },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "tester",
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "tester"
        };

        var recurrences = forecast.GetRecurrences(new DateOnly(2026, 3, 1), new DateOnly(2026, 5, 1));

        Assert.That(recurrences, Has.Count.EqualTo(1));
        Assert.That(recurrences[0], Is.EqualTo(new DateOnly(2026, 4, 1)));
    }
}
