using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandValidatorTests
{
    private readonly CreateForecastDefinitionCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Description_Is_Empty()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = string.Empty,
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Is_Null()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = null!,
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Is_Whitespace()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "   ",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_100_Characters()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = new string('A', 101),
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 100 characters");
    }

    [Fact]
    public void Should_Succeed_When_Description_Is_Valid()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Valid Description",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Succeed_When_Description_Is_1_Character()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "A",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Succeed_When_Description_Is_Exactly_100_Characters()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = new string('A', 100),
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 0m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = -50m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Amount_Has_More_Than_2_Decimal_Places()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100.123m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must have maximum 2 decimal places");
    }

    [Fact]
    public void Should_Succeed_When_Amount_Has_2_Decimal_Places()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100.99m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_Amount_Has_No_Decimal_Places()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_Amount_Has_1_Decimal_Place()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100.5m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Interval_Is_Zero()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 0,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Interval)
            .WithErrorMessage("Interval must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Interval_Is_Negative()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = -1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Interval)
            .WithErrorMessage("Interval must be greater than 0");
    }

    [Fact]
    public void Should_Succeed_When_Interval_Is_Positive()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Interval);
    }

    [Fact]
    public void Should_Succeed_When_Interval_Is_Large_Positive_Value()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 365,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Interval);
    }

    [Fact]
    public void Should_Fail_When_RecurrenceEnd_Is_Earlier_Than_RecurrenceStart()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = new DateOnly(2024, 12, 31),
            RecurrenceEnd = new DateOnly(2024, 1, 1),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RecurrenceEnd)
            .WithErrorMessage("Recurrence end date cannot be earlier than recurrence start date.");
    }

    [Fact]
    public void Should_Succeed_When_RecurrenceEnd_Equals_RecurrenceStart()
    {
        var startDate = new DateOnly(2024, 6, 15);
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = startDate,
            RecurrenceEnd = startDate,
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.RecurrenceEnd);
    }

    [Fact]
    public void Should_Succeed_When_RecurrenceEnd_Is_Later_Than_RecurrenceStart()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.RecurrenceEnd);
    }

    [Fact]
    public void Should_Succeed_When_RecurrenceEnd_Is_Null()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = null,
            IsIncome = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.RecurrenceEnd);
    }

    [Fact]
    public void Should_Fail_When_PaymentCategoryId_Is_Null_And_IsIncome_Is_False()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = false,
            PaymentCategoryId = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentCategoryId)
            .WithErrorMessage("Expense forecasts require a payment category.");
    }

    [Fact]
    public void Should_Succeed_When_PaymentCategoryId_Has_Value_And_IsIncome_Is_False()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = false,
            PaymentCategoryId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Succeed_When_PaymentCategoryId_Is_Null_And_IsIncome_Is_True()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true,
            PaymentCategoryId = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Succeed_When_PaymentCategoryId_Is_Empty_And_IsIncome_Is_True()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = true,
            PaymentCategoryId = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Succeed_When_All_Properties_Are_Valid()
    {
        var command = new CreateForecastDefinitionCommand
        {
            Description = "Valid Forecast",
            Amount = 150.50m,
            Interval = 7,
            RecurrenceStart = new DateOnly(2024, 1, 1),
            RecurrenceEnd = new DateOnly(2024, 12, 31),
            IsIncome = false,
            PaymentCategoryId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
