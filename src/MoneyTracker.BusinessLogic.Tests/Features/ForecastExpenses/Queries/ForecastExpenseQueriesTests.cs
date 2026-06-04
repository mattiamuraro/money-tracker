using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.Queries;

public class ForecastExpenseQueriesTests
{
    [Fact]
    public void GetForecastExpenseDefinitionsQuery_ShouldCreateInstance()
    {
        var query = new GetForecastExpenseDefinitionsQuery();
        Assert.NotNull(query);
    }

    [Fact]
    public void GetForecastExpenseRowsQuery_ShouldSetStartAndEndDates()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 31);

        var query = new GetForecastExpenseRowsQuery(start, end);

        Assert.Equal(start, query.StartDate);
        Assert.Equal(end, query.EndDate);
    }

    [Fact]
    public void GetPendingForecastExpenseOccurrencesQuery_ShouldSetYearAndMonth()
    {
        var query = new GetPendingForecastExpenseOccurrencesQuery(2026, 3);

        Assert.Equal(2026, query.Year);
        Assert.Equal(3, query.Month);
    }
}
