using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.CreateForecastExpenseDefinition;

public class CreateForecastExpenseDefinitionCommandValidatorTests
{
    private readonly CreateForecastExpenseDefinitionCommandValidator _validator = new();

    private static CreateForecastExpenseDefinitionCommand ValidCommand() => new()
    {
        Description = "Rent",
        Amount = 800m,
        Interval = 1,
        ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
        RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
        PaymentCategoryId = Guid.NewGuid()
    };

    [Fact]
    public void Should_Pass_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_WhenDescriptionIsEmptyOrWhitespace(string description)
    {
        var command = ValidCommand();
        command.Description = description;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_WhenAmountIsZero()
    {
        var command = ValidCommand();
        command.Amount = 0;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_WhenAmountIsNegative()
    {
        var command = ValidCommand();
        command.Amount = -1m;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_WhenIntervalIsZero()
    {
        var command = ValidCommand();
        command.Interval = 0;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Interval);
    }

    [Fact]
    public void Should_Fail_WhenPaymentCategoryIdIsEmpty()
    {
        var command = ValidCommand();
        command.PaymentCategoryId = Guid.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PaymentCategoryId);
    }

    [Fact]
    public void Should_Fail_WhenRecurrenceEndIsBeforeRecurrenceStart()
    {
        var command = ValidCommand();
        command.RecurrenceStart = new DateOnly(2025, 6, 1);
        command.RecurrenceEnd = new DateOnly(2025, 5, 1);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RecurrenceEnd);
    }
}
