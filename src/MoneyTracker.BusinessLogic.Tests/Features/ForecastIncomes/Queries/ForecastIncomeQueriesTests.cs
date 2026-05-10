using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.Queries;

public class ForecastIncomeQueriesTests
{
    [Fact]
    public void GetForecastIncomeDefinitionsQuery_ShouldCreateInstance()
    {
        var query = new GetForecastIncomeDefinitionsQuery();
        Assert.NotNull(query);
    }

    [Fact]
    public void GetForecastIncomeRowsQuery_ShouldSetStartAndEndDates()
    {
        var start = new DateOnly(2026, 2, 1);
        var end = new DateOnly(2026, 2, 28);

        var query = new GetForecastIncomeRowsQuery(start, end);

        Assert.Equal(start, query.StartDate);
        Assert.Equal(end, query.EndDate);
    }

    [Fact]
    public void GetPendingForecastIncomeOccurrencesQuery_ShouldSetYearAndMonth()
    {
        var query = new GetPendingForecastIncomeOccurrencesQuery(2026, 4);

        Assert.Equal(2026, query.Year);
        Assert.Equal(4, query.Month);
    }
}
