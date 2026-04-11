using Xunit;
using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Payments.Validators;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.UpdatePayment;

namespace MoneyTracker.Tests.Features.Payments.Validators;

/// <summary>
/// Unit tests for UpdatePaymentCommandValidator
/// </summary>
public class UpdatePaymentCommandValidatorTests
{
    private readonly UpdatePaymentCommandValidator _validator;

    public UpdatePaymentCommandValidatorTests()
    {
        _validator = new UpdatePaymentCommandValidator();
    }

    [Fact]
    public void Should_Fail_When_PaymentId_Is_Empty()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.Empty,
            Description = "Updated Payment",
            Amount = 150,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }

    [Fact]
    public void Should_Fail_When_ModifiedBy_Is_Empty()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = "Updated Payment",
            Amount = 150,
            ModifiedById = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x .ModifiedById);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_Max_Length()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = new string('a', 101),
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = -50,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Date_Is_In_Future()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(1),
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Succeed_With_Only_Required_Fields()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Succeed_With_Partial_Update()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = "Updated Description",
            Amount = 200,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Allow_Empty_Optional_Fields()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = null,
            Amount = null,
            Date = null,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
