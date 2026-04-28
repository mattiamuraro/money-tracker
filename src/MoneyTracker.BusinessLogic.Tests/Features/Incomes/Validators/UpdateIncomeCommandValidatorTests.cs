using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.Validators;

public class UpdateIncomeCommandValidatorTests
{
    private readonly UpdateIncomeCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_IncomeId_Is_Empty()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IncomeId);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_Max_Length()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = new string('a', 101)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Validate_Description_When_Null()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = -100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Not_Validate_Amount_When_Null()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_With_Only_Required_Fields()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Succeed_With_All_Optional_Fields()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = "Updated salary",
            Amount = 3000,
            Date = DateTime.UtcNow
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Validate_Description_When_Whitespace()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Validate_Description_When_Empty()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Succeed_When_Description_Is_Exactly_100_Characters()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = new string('a', 100)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Has_More_Than_2_Decimal_Places()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = 100.123m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_Amount_Has_Exactly_2_Decimal_Places()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = 100.12m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_Amount_Has_1_Decimal_Place()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = 100.1m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_Amount_Is_Positive_Integer()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Amount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_When_IncomeId_Is_Valid_Guid()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.IncomeId);
    }
}
