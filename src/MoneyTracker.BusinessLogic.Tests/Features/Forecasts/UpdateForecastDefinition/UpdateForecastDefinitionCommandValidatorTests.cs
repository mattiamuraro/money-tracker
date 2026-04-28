using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;

namespace MoneyTracker.BusinessLogic.Tests.Features.Forecasts.UpdateForecastDefinition;

public class UpdateForecastDefinitionCommandValidatorTests
{
    private readonly UpdateForecastDefinitionCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_RecurrenceEnd_Is_Earlier_Than_RecurrenceStart()
    {
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
    public void Should_Fail_When_PaymentCategoryId_Is_Empty_And_IsIncome_Is_False()
    {
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Test",
            Amount = 100m,
            Interval = 1,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            IsIncome = false,
            PaymentCategoryId = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentCategoryId)
            .WithErrorMessage("Expense forecasts require a payment category.");
    }

    [Fact]
    public void Should_Fail_When_PaymentCategoryId_Is_Null_And_IsIncome_Is_False()
    {
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
        var command = new UpdateForecastDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
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
}
