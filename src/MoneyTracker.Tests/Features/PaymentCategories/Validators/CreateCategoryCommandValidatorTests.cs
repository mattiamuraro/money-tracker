using Xunit;
using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Validators;

namespace MoneyTracker.Tests.Features.PaymentCategories.Validators;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateCategoryCommand
        {
            Name = string.Empty,
            Code = "FOOD",
            CreatedBy = "tester"
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
            Code = "food-1",
            CreatedBy = "tester"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_Fail_When_CreatedBy_Is_Empty()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD",
            CreatedBy = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CreatedBy);
    }

    [Fact]
    public void Should_Succeed_When_Command_Is_Valid()
    {
        var command = new CreateCategoryCommand
        {
            Name = "Food",
            Code = "FOOD",
            CreatedBy = "tester"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
