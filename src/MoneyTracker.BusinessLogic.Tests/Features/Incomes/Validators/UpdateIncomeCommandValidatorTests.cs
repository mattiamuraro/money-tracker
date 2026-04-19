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
            IncomeId = Guid.Empty,
            ModifiedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IncomeId);
    }

    [Fact]
    public void Should_Fail_When_ModifiedById_Is_Empty()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            ModifiedById = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ModifiedById);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_Max_Length()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            Description = new string('a', 101),
            ModifiedById = Guid.NewGuid()
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
            Description = null,
            ModifiedById = Guid.NewGuid()
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
            Amount = 0,
            ModifiedById = Guid.NewGuid()
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
            Amount = -100,
            ModifiedById = Guid.NewGuid()
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
            Amount = null,
            ModifiedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Succeed_With_Only_Required_Fields()
    {
        var command = new UpdateIncomeCommand
        {
            IncomeId = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid()
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
            Date = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
