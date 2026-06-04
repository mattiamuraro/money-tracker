using Xunit;
using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;

namespace MoneyTracker.BusinessLogic.Tests.Features.PaymentCategories.Validators;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateCategoryCommand
        {
            Name = string.Empty,
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Code_Is_Not_Uppercase_Alphanumeric()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "food-1"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_Succeed_When_Command_Is_Valid()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Null()
    {
        var command = new CreateCategoryCommand
        {
            Name = null!,
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name is required.");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_50_Characters()
    {
        var command = new CreateCategoryCommand
        {
            Name = new string('A', 51),
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name cannot exceed 50 characters.");
    }

    [Fact]
    public void Should_Succeed_When_Name_Is_Exactly_50_Characters()
    {
        var command = new CreateCategoryCommand
        {
            Name = new string('A', 50),
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Code_Is_Null()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = null!
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code is required.");
    }

    [Fact]
    public void Should_Fail_When_Code_Is_Empty()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code is required.");
    }

    [Fact]
    public void Should_Fail_When_Code_Exceeds_5_Characters()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD12"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code cannot exceed 5 characters.");
    }

    [Fact]
    public void Should_Succeed_When_Code_Is_Exactly_5_Characters()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD1"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Code_Contains_Lowercase_Letters()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "Food"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code must contain only uppercase letters and numbers.");
    }

    [Fact]
    public void Should_Fail_When_Code_Contains_Special_Characters()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOO-D"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code must contain only uppercase letters and numbers.");
    }

    [Fact]
    public void Should_Fail_When_Code_Contains_Spaces()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FO OD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Category code must contain only uppercase letters and numbers.");
    }

    [Fact]
    public void Should_Succeed_When_Code_Contains_Only_Uppercase_Letters()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Succeed_When_Code_Contains_Only_Numbers()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "12345"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Succeed_When_Code_Contains_Uppercase_Letters_And_Numbers()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "CAT1"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Whitespace_Only()
    {
        var command = new CreateCategoryCommand
        {
            Name = "   ",
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name is required.");
    }

    [Fact]
    public void Should_Fail_When_Code_Is_Whitespace_Only()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Constructor_InitializesValidatorWithAllRules()
    {
        var validator = new CreateCategoryCommandValidator();
        var validCommand = new CreateCategoryCommand { Name = "Food", Code = "FOOD" };
        var invalidCommand = new CreateCategoryCommand { Name = null!, Code = null! };

        var validResult = validator.TestValidate(validCommand);
        var invalidResult = validator.TestValidate(invalidCommand);

        validResult.ShouldNotHaveAnyValidationErrors();
        invalidResult.ShouldHaveValidationErrorFor(x => x.Name);
        invalidResult.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_Succeed_When_Name_Is_Single_Character()
    {
        var command = new CreateCategoryCommand
        {
            Name = "A",
            Code = "FOOD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Succeed_When_Code_Is_Single_Character()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "A"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
