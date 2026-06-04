using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

namespace MoneyTracker.BusinessLogic.Tests.Shared.Models;

public class BusinessLogicDtoCoverageTests
{
    [Fact]
    public void LoginAuthTokenDto_ShouldStoreToken()
    {
        var dto = new LoginAuthTokenDto { Token = "abc", RefreshToken = "ref-abc" };
        Assert.Equal("abc", dto.Token);
        Assert.Equal("ref-abc", dto.RefreshToken);
    }

    [Fact]
    public void RegisterAuthTokenDto_ShouldStoreToken()
    {
        var dto = new RegisterAuthTokenDto { Token = "xyz", RefreshToken = "ref-xyz" };
        Assert.Equal("xyz", dto.Token);
        Assert.Equal("ref-xyz", dto.RefreshToken);
    }

    [Fact]
    public void PaymentDto_ShouldStoreProperties()
    {
        var id = Guid.NewGuid();
        var dto = new PaymentDto
        {
            Id = id,
            Description = "d",
            PaymentCategoryId = Guid.NewGuid(),
            Category = "c",
            ForecastOccurrenceId = Guid.NewGuid(),
            ForecastExpectedDate = new DateOnly(2026, 1, 10),
            Amount = 10m,
            Date = DateTime.UtcNow,
            IsOneShot = true
        };

        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void IncomeDto_ShouldStoreProperties()
    {
        var id = Guid.NewGuid();
        var dto = new IncomeDto
        {
            Id = id,
            Description = "income",
            ForecastOccurrenceId = Guid.NewGuid(),
            ForecastExpectedDate = new DateOnly(2026, 1, 10),
            Amount = 30m,
            Date = DateTime.UtcNow
        };

        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void PaymentCategoryDto_ShouldStoreProperties()
    {
        var id = Guid.NewGuid();
        var dto = new PaymentCategoryDto { Id = id, Name = "Food", Code = "FOOD", CreatedAt = DateTime.UtcNow };

        Assert.Equal(id, dto.Id);
    }

    [Fact]
    public void ForecastDtos_ShouldStoreProperties()
    {
        var expenseDef = new ForecastExpenseDefinitionDto
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "rent",
            Amount = 1000m,
            RecurrenceStart = new DateOnly(2026, 1, 1),
            RecurrenceEnd = new DateOnly(2026, 12, 1),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid(),
            Category = "Home"
        };

        var incomeDef = new ForecastIncomeDefinitionDto
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "salary",
            Amount = 3000m,
            RecurrenceStart = new DateOnly(2026, 1, 1),
            RecurrenceEnd = new DateOnly(2026, 12, 1),
            Interval = 1
        };

        var expense = new ForecastExpenseDto
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "rent-occ",
            Amount = 1000m,
            Date = new DateOnly(2026, 2, 1),
            PaymentCategoryId = Guid.NewGuid(),
            Category = "Home"
        };

        var income = new ForecastIncomeDto
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "salary-occ",
            Amount = 3000m,
            Date = new DateOnly(2026, 2, 1)
        };

        var expenseOccurrence = new ForecastExpenseOccurrenceDto
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "rent-pending",
            Amount = 1000m,
            ExpectedDate = new DateOnly(2026, 2, 1),
            PaymentCategoryId = Guid.NewGuid(),
            Category = "Home"
        };

        var incomeOccurrence = new ForecastIncomeOccurrenceDto
        {
            Id = Guid.NewGuid(),
            ForecastDefinitionId = Guid.NewGuid(),
            Description = "salary-pending",
            Amount = 3000m,
            ExpectedDate = new DateOnly(2026, 2, 1)
        };

        var recurrenceDto = new ForecastRecurrenceRuleTypeDto
        {
            Id = Guid.NewGuid(),
            Name = "Month",
            Code = "M"
        };

        Assert.Equal("M", recurrenceDto.Code);
        Assert.Equal(expenseDef.Interval, incomeDef.Interval);
        Assert.Equal(expense.Date, income.Date);
        Assert.Equal(expenseOccurrence.ExpectedDate, incomeOccurrence.ExpectedDate);
    }
}
