using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.SynchronizeForecastOccurrences;

public class SynchronizeForecastOccurrencesCommandTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var command = new SynchronizeForecastOccurrencesCommand();
        Assert.NotNull(command);
    }
}
