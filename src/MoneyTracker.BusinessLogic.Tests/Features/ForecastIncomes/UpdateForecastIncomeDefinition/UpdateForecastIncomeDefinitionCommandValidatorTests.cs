using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.UpdateForecastIncomeDefinition;

public class UpdateForecastIncomeDefinitionCommandValidatorTests
{
    private readonly UpdateForecastIncomeDefinitionCommandValidator _validator = new();

    private static UpdateForecastIncomeDefinitionCommand ValidCommand() => new()
    {
        Id = Guid.NewGuid(),
        Description = "Monthly salary",
        Amount = 3000m,
        Interval = 1,
        ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
        RecurrenceStart = new DateOnly(2024, 1, 1)
    };

    [Fact]
    public void Should_Pass_WhenCommandIsValid()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_WhenRecurrenceEndIsNull()
    {
        var command = ValidCommand();
        command.RecurrenceEnd = null;
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_WhenRecurrenceEndEqualsRecurrenceStart()
    {
        var command = ValidCommand();
        command.RecurrenceStart = new DateOnly(2024, 6, 1);
        command.RecurrenceEnd = new DateOnly(2024, 6, 1);
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.RecurrenceEnd);
    }

    // ─────────────────────────────────────── Description ──

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
    public void Should_Fail_WhenDescriptionExceeds100Characters()
    {
        var command = ValidCommand();
        command.Description = new string('x', 101);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Description);
    }

    // ────────────────────────────────────────── Amount ──

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
    public void Should_Fail_WhenAmountHasMoreThanTwoDecimalPlaces()
    {
        var command = ValidCommand();
        command.Amount = 10.999m;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    // ────────────────────────────────────────── Interval ──

    [Fact]
    public void Should_Fail_WhenIntervalIsZero()
    {
        var command = ValidCommand();
        command.Interval = 0;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Interval);
    }

    [Fact]
    public void Should_Fail_WhenIntervalIsNegative()
    {
        var command = ValidCommand();
        command.Interval = -1;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Interval);
    }

    // ─────────────────────────────────────── RecurrenceEnd ──

    [Fact]
    public void Should_Fail_WhenRecurrenceEndIsBeforeRecurrenceStart()
    {
        var command = ValidCommand();
        command.RecurrenceStart = new DateOnly(2025, 6, 1);
        command.RecurrenceEnd = new DateOnly(2025, 5, 1);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RecurrenceEnd);
    }
}
