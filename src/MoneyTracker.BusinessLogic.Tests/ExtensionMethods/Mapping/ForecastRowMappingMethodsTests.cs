using Xunit;
using MoneyTracker.BusinessLogic.ExtensionMethods.Mapping;
using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.ExtensionMethods.Mapping;

public class ForecastRowMappingMethodsTests
{
    [Fact]
    public void ToForecastRow_Should_Map_All_Properties()
    {
        var actorId = Guid.NewGuid();

        var ruleType = new ForecastRecurrenceRuleType
        {
            Id = Guid.NewGuid(),
            Name = "Month",
            Code = ForecastRecurrenceRuleType.Month,
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        var forecast = new ForecastExpense
        {
            Id = Guid.NewGuid(),
            Description = "Electricity",
            Amount = 75.25m,
            RecurrenceStart = new DateOnly(2026, 4, 1),
            Interval = 1,
            ForecastRecurrenceRuleTypeId = ruleType.Id,
            ForecastRecurrenceRuleType = ruleType,
            CreatedAt = DateTime.UtcNow,
            CreatedById = actorId,
            ModifiedAt = DateTime.UtcNow,
            ModifiedById = actorId
        };

        var date = new DateOnly(2026, 4, 15);

        var row = forecast.ToForecastRow(date, isIncome: true);

        Xunit.Assert.Equal(forecast.Id, row.Id);
        Xunit.Assert.Equal("Electricity", row.Description);
        Xunit.Assert.Equal(75.25m, row.Amount);
        Xunit.Assert.Equal(date, row.Date);
        Xunit.Assert.True(row.IsIncome);
    }
}
