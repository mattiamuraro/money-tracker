using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Validators;
using Xunit;

namespace MoneyTracker.Tests.Features.PaymentCategories.Validators;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Id_Is_Empty()
    {
        var command = new UpdateCategoryCommand
        {
            Id = Guid.Empty,
            Name = "Food",
            Code = "FOOD",
            ModifiedBy = "tester"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Fail_When_Code_Is_Invalid()
    {
        var command = new UpdateCategoryCommand
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "food",
            ModifiedBy = "tester"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_Fail_When_ModifiedBy_Is_Empty()
    {
        var command = new UpdateCategoryCommand
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            ModifiedBy = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ModifiedBy);
    }

    [Fact]
    public void Should_Succeed_When_Command_Is_Valid()
    {
        var command = new UpdateCategoryCommand
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Code = "FOOD",
            ModifiedBy = "tester"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
