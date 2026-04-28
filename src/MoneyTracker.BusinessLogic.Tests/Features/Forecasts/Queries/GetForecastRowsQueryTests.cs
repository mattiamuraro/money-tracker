using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.Queries;

public class GetForecastRowsQueryTests
{
    [Fact]
    public void Constructor_Should_Map_All_Properties()
    {
        var startDate = new DateOnly(2026, 1, 1);
        var endDate = new DateOnly(2026, 12, 31);

        var query = new GetForecastRowsQuery(startDate, endDate);

        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
    }

    [Fact]
    public void Constructor_Should_Allow_Same_Start_And_End_Date()
    {
        var date = new DateOnly(2026, 6, 15);

        var query = new GetForecastRowsQuery(date, date);

        Assert.Equal(date, query.StartDate);
        Assert.Equal(date, query.EndDate);
    }

    [Fact]
    public void Constructor_Should_Allow_End_Date_Before_Start_Date()
    {
        var startDate = new DateOnly(2026, 12, 31);
        var endDate = new DateOnly(2026, 1, 1);

        var query = new GetForecastRowsQuery(startDate, endDate);

        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
    }

    [Fact]
    public void Constructor_Should_Handle_Boundary_Min_Date()
    {
        var startDate = DateOnly.MinValue;
        var endDate = new DateOnly(2026, 1, 1);

        var query = new GetForecastRowsQuery(startDate, endDate);

        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
    }

    [Fact]
    public void Constructor_Should_Handle_Boundary_Max_Date()
    {
        var startDate = new DateOnly(2026, 1, 1);
        var endDate = DateOnly.MaxValue;

        var query = new GetForecastRowsQuery(startDate, endDate);

        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
    }

    [Fact]
    public void Constructor_Should_Handle_Dates_Spanning_Multiple_Years()
    {
        var startDate = new DateOnly(2020, 1, 1);
        var endDate = new DateOnly(2030, 12, 31);

        var query = new GetForecastRowsQuery(startDate, endDate);

        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
    }
}
