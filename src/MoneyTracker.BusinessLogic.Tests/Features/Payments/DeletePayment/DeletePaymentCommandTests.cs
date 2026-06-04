using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.DeletePayment;

public class DeletePaymentCommandTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateInstance_WithDefaultValues()
    {
        // Arrange & Act
        var command = new DeletePaymentCommand();

        // Assert
        Xunit.Assert.Equal(Guid.Empty, command.PaymentId);
        Xunit.Assert.Null(command.OccurrenceAction);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPaymentIdProperty_WhenCalledWithValidGuid()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        string? occurrenceAction = "single";

        // Act
        var command = new DeletePaymentCommand(expectedId, occurrenceAction);

        // Assert
        Xunit.Assert.Equal(expectedId, command.PaymentId);
        Xunit.Assert.Equal(occurrenceAction, command.OccurrenceAction);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPaymentIdProperty_WhenCalledWithEmptyGuid()
    {
        // Arrange
        var expectedId = Guid.Empty;
        string? occurrenceAction = null;

        // Act
        var command = new DeletePaymentCommand(expectedId, occurrenceAction);

        // Assert
        Xunit.Assert.Equal(expectedId, command.PaymentId);
        Xunit.Assert.Null(command.OccurrenceAction);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetPaymentIdProperty_WhenCalledWithSpecificGuid()
    {
        // Arrange
        var expectedId = new Guid("12345678-1234-1234-1234-123456789012");
        string? occurrenceAction = "all";

        // Act
        var command = new DeletePaymentCommand(expectedId, occurrenceAction);

        // Assert
        Xunit.Assert.Equal(expectedId, command.PaymentId);
        Xunit.Assert.Equal(occurrenceAction, command.OccurrenceAction);
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetOccurrenceActionToNull_WhenCalledWithNullValue()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        string? occurrenceAction = null;

        // Act
        var command = new DeletePaymentCommand(paymentId, occurrenceAction);

        // Assert
        Xunit.Assert.Equal(paymentId, command.PaymentId);
        Xunit.Assert.Null(command.OccurrenceAction);
    }
}
