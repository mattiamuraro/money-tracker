using Xunit;
using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.BusinessLogic.Tests.Features.Payments.Validators;

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
    public void Should_Allow_When_Date_Is_In_Future()
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
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
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

    [Fact]
    public void Should_Pass_When_Description_Is_Empty_String()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = string.Empty,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Pass_When_Description_Is_Exactly_100_Characters()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Description = new string('a', 100),
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_PaymentCategoryId_Is_Empty_Guid()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentCategoryId = Guid.Empty,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Pass_When_PaymentCategoryId_Is_Null()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentCategoryId = null,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 0,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Amount_Has_More_Than_Two_Decimal_Places()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.123m,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Pass_When_Amount_Has_Two_Decimal_Places()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.12m,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Pass_When_Amount_Is_Null()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = null,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Pass_When_Amount_Has_One_Decimal_Place()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.5m,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Pass_When_Amount_Has_No_Decimal_Places()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100m,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Pass_When_PaymentCategoryId_Has_Valid_Value()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentCategoryId = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_PaymentId()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.Empty,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentId)
            .WithErrorMessage("Payment ID is required");
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_Description_Length()
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
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_PaymentCategoryId()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentCategoryId = Guid.Empty,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentCategoryId)
            .WithErrorMessage("Payment category is required");
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_Amount_GreaterThan()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 0,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than 0");
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_Amount_PrecisionScale()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.123m,
            ModifiedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must have maximum 2 decimal places");
    }

    [Fact]
    public void Should_Have_Correct_Error_Message_For_ModifiedById()
    {
        // Arrange
        var command = new UpdatePaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            ModifiedById = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x .ModifiedById)
            .WithErrorMessage("ModifiedBy is required");
    }
}
