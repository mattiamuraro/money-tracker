using Xunit;

namespace MoneyTracker.Data.Tests;

public class BaseForecastTests
{
    [Fact]
    public void GetRecurrences_Should_Return_Expected_Dates_In_Range()
    {
        var actorId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = Guid.NewGuid(),
            Name = "Week",
            Code = ForecastRecurrenceRuleType.Week
        };

        var forecast = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Subscription",
            Amount = 10m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleType.Id,
            ForecastRecurrenceRuleType = ruleType
        };

        var recurrences = forecast.GetRecurrences(new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 25));

        Assert.Equal(new List<DateOnly>
        {
            new(2026, 4, 8),
            new(2026, 4, 15),
            new(2026, 4, 22)
        }, recurrences);
    }

    [Fact]
    public void GetRecurrences_Should_Stop_When_No_Next_Occurrence()
    {
        var actorId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = Guid.NewGuid(),
            Name = "One Time",
            Code = ForecastRecurrenceRuleType.OneTime
        };

        var forecast = new ForecastIncome
        {
            Id = Guid.NewGuid(),
            Description = "One time",
            Amount = 100m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleType.Id,
            ForecastRecurrenceRuleType = ruleType
        };

        var recurrences = forecast.GetRecurrences(new DateOnly(2026, 3, 1), new DateOnly(2026, 5, 1));

        Assert.Single(recurrences);
        Assert.Equal(new DateOnly(2026, 4, 1), recurrences[0]);
    }
}
