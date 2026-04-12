using Xunit;
using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Payments.Validators;
using MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;

namespace MoneyTracker.Tests.Features.Payments.Validators;

/// <summary>
/// Unit tests for CreatePaymentCommandValidator
/// </summary>
public class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _validator;

    public CreatePaymentCommandValidatorTests()
    {
        _validator = new CreatePaymentCommandValidator();
    }

    [Fact]
    public void Should_Fail_When_Description_Is_Empty()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = string.Empty,
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_Max_Length()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = new string('a', 101),
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 0,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = -50.00m,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
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
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Fail_When_PaymentCategoryId_Is_Empty()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.Empty,
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Fail_When_CreatedBy_Is_Empty()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.Empty,
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x .CreatedById);
    }

    [Fact]
    public void Should_Succeed_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Xunit.Theory]
    [Xunit.InlineData(0.01)]
    [Xunit.InlineData(99.99)]
    [Xunit.InlineData(1000.00)]
    public void Should_Accept_Valid_Amounts(decimal amount)
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            Description = "Test Payment",
            PaymentCategoryId = Guid.NewGuid(),
            Amount = amount,
            Date = DateTime.UtcNow.AddDays(-1),
            CreatedById = Guid.NewGuid(),
            IsOneShot = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
