using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.DeletePayment.ExtensionMethods;

public class DeletePaymentHelpersTests
{
    [Fact]
    public void TryParseOccurrenceAction_WithNull_ShouldReturnAutoAndTrue()
    {
        var method = GetMethod();
        object?[] args = [null, null];

        var parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal(ForecastOccurrenceDeleteAction.Auto, (ForecastOccurrenceDeleteAction)args[1]!);
    }

    [Fact]
    public void TryParseOccurrenceAction_WithValidValue_ShouldParseCaseInsensitive()
    {
        var method = GetMethod();
        object?[] args = ["reopen", null];

        var parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal(ForecastOccurrenceDeleteAction.Reopen, (ForecastOccurrenceDeleteAction)args[1]!);
    }

    [Fact]
    public void TryParseOccurrenceAction_WithInvalidValue_ShouldReturnFalse()
    {
        var method = GetMethod();
        object?[] args = ["unknown", null];

        var parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
    }

    private static System.Reflection.MethodInfo GetMethod()
    {
        var type = typeof(MoneyTracker.BusinessLogic.Common.Extensions.BusinessLogicServiceCollectionExtensions)
            .Assembly
            .GetType("MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.ExtensionMethods.DeletePaymentHelpers");
        Assert.NotNull(type);

        var method = type!.GetMethod("TryParseOccurrenceAction", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        Assert.NotNull(method);
        return method!;
    }
}
