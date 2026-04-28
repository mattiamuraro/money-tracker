using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.Queries;

public class GetPendingForecastOccurrencesQueryTests
{
    [Fact]
    public void Constructor_Should_Map_All_Properties()
    {
        var query = new GetPendingForecastOccurrencesQuery(year: 2026, month: 4, isIncome: true);

        Assert.Equal(2026, query.Year);
        Assert.Equal(4, query.Month);
        Assert.True(query.IsIncome);
    }

    [Fact]
    public void Constructor_Should_Map_IsIncome_False()
    {
        var query = new GetPendingForecastOccurrencesQuery(year: 2025, month: 12, isIncome: false);

        Assert.Equal(2025, query.Year);
        Assert.Equal(12, query.Month);
        Assert.False(query.IsIncome);
    }

    [Fact]
    public void Constructor_WithMinimumValues_ShouldInitializeCorrectly()
    {
        var query = new GetPendingForecastOccurrencesQuery(year: 1, month: 1, isIncome: false);

        Assert.Equal(1, query.Year);
        Assert.Equal(1, query.Month);
        Assert.False(query.IsIncome);
    }

    [Fact]
    public void Constructor_WithZeroValues_ShouldInitializeCorrectly()
    {
        var query = new GetPendingForecastOccurrencesQuery(year: 0, month: 0, isIncome: true);

        Assert.Equal(0, query.Year);
        Assert.Equal(0, query.Month);
        Assert.True(query.IsIncome);
    }

    [Fact]
    public void Constructor_WithNegativeValues_ShouldInitializeCorrectly()
    {
        var query = new GetPendingForecastOccurrencesQuery(year: -1, month: -5, isIncome: false);

        Assert.Equal(-1, query.Year);
        Assert.Equal(-5, query.Month);
        Assert.False(query.IsIncome);
    }
}
