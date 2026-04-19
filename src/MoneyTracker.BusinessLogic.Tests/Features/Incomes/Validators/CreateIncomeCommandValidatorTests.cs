using FluentValidation.TestHelper;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

namespace MoneyTracker.BusinessLogic.Tests.Features.Incomes.Validators;

public class CreateIncomeCommandValidatorTests
{
    private readonly CreateIncomeCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Description_Is_Empty()
    {
        var command = new CreateIncomeCommand
        {
            Description = string.Empty,
            Amount = 100,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_Max_Length()
    {
        var command = new CreateIncomeCommand
        {
            Description = new string('a', 101),
            Amount = 100,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 0,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = -50,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Fail_When_Date_Is_Empty()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 100,
            Date = default,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Fail_When_CreatedById_Is_Empty()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 100,
            Date = DateTime.UtcNow,
            CreatedById = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CreatedById);
    }

    [Fact]
    public void Should_Fail_When_IdempotencyKey_Exceeds_Max_Length()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 100,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            IdempotencyKey = new string('k', 257)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void Should_Not_Validate_IdempotencyKey_When_Null()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 100,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            IdempotencyKey = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void Should_Succeed_When_All_Fields_Are_Valid()
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = 2500,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            IdempotencyKey = "unique-key-123"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999.99)]
    [InlineData(10000)]
    public void Should_Accept_Valid_Amounts(decimal amount)
    {
        var command = new CreateIncomeCommand
        {
            Description = "Salary",
            Amount = amount,
            Date = DateTime.UtcNow,
            CreatedById = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
