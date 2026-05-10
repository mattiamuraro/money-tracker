namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.Shared;

public class ForecastSynchronizationWindowTests
{
    [Fact]
    public void GetWindow_ShouldReturnFirstDayOfCurrentMonth_AndThreeMonthRange()
    {
        var type = typeof(MoneyTracker.BusinessLogic.Common.Extensions.BusinessLogicServiceCollectionExtensions)
            .Assembly
            .GetType("MoneyTracker.BusinessLogic.Features.Forecasts.Shared.ForecastSynchronizationWindow");
        Assert.NotNull(type);

        var method = type!.GetMethod("GetWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);

        var tuple = method!.Invoke(null, null)!;
        var startDate = (DateOnly)tuple.GetType().GetField("Item1")!.GetValue(tuple)!;
        var endDate = (DateOnly)tuple.GetType().GetField("Item2")!.GetValue(tuple)!;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var expectedStart = new DateOnly(today.Year, today.Month, 1);
        var expectedEnd = expectedStart.AddMonths(3).AddDays(-1);

        Assert.Equal(expectedStart, startDate);
        Assert.Equal(expectedEnd, endDate);
    }
}
