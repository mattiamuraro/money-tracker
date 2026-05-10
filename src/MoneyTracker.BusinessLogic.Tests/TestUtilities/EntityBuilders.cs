using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.TestUtilities;

internal sealed class ForecastRecurrenceRuleTypeBuilder
{
    private readonly ForecastRecurrenceRuleType _entity = new()
    {
        Id = Guid.NewGuid(),
        Name = "Month",
        Code = ForecastRecurrenceRuleType.Month
    };

    internal ForecastRecurrenceRuleTypeBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    internal ForecastRecurrenceRuleTypeBuilder WithName(string name)
    {
        _entity.Name = name;
        return this;
    }

    internal ForecastRecurrenceRuleTypeBuilder WithCode(string code)
    {
        _entity.Code = code;
        return this;
    }

    internal ForecastRecurrenceRuleType Build() => _entity;
}

internal sealed class PaymentCategoryBuilder
{
    private readonly PaymentCategory _entity = new()
    {
        Id = Guid.NewGuid(),
        Name = "Home",
        Code = "HOME"
    };

    internal PaymentCategoryBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    internal PaymentCategoryBuilder WithName(string name)
    {
        _entity.Name = name;
        return this;
    }

    internal PaymentCategoryBuilder WithCode(string code)
    {
        _entity.Code = code;
        return this;
    }

    internal PaymentCategory Build() => _entity;
}

internal sealed class ForecastExpenseBuilder
{
    private readonly ForecastExpense _entity = new()
    {
        Id = Guid.NewGuid(),
        Description = "Default expense",
        Amount = 10m,
        RecurrenceStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
        RecurrenceEnd = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
        Interval = 1,
        ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
        PaymentCategoryId = Guid.NewGuid(),
        IsActive = true
    };

    internal ForecastExpenseBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    internal ForecastExpenseBuilder WithDescription(string description)
    {
        _entity.Description = description;
        return this;
    }

    internal ForecastExpenseBuilder WithAmount(decimal amount)
    {
        _entity.Amount = amount;
        return this;
    }

    internal ForecastExpenseBuilder WithRecurrence(DateOnly start, DateOnly? end, int interval)
    {
        _entity.RecurrenceStart = start;
        _entity.RecurrenceEnd = end;
        _entity.Interval = interval;
        return this;
    }

    internal ForecastExpenseBuilder WithRecurrenceRuleTypeId(Guid recurrenceRuleTypeId)
    {
        _entity.ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId;
        return this;
    }

    internal ForecastExpenseBuilder WithPaymentCategoryId(Guid paymentCategoryId)
    {
        _entity.PaymentCategoryId = paymentCategoryId;
        return this;
    }

    internal ForecastExpenseBuilder WithIsActive(bool isActive)
    {
        _entity.IsActive = isActive;
        return this;
    }

    internal ForecastExpense Build() => _entity;
}

internal sealed class ForecastIncomeBuilder
{
    private readonly ForecastIncome _entity = new()
    {
        Id = Guid.NewGuid(),
        Description = "Default income",
        Amount = 100m,
        RecurrenceStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
        RecurrenceEnd = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
        Interval = 1,
        ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
        IsActive = true
    };

    internal ForecastIncomeBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    internal ForecastIncomeBuilder WithDescription(string description)
    {
        _entity.Description = description;
        return this;
    }

    internal ForecastIncomeBuilder WithAmount(decimal amount)
    {
        _entity.Amount = amount;
        return this;
    }

    internal ForecastIncomeBuilder WithRecurrence(DateOnly start, DateOnly? end, int interval)
    {
        _entity.RecurrenceStart = start;
        _entity.RecurrenceEnd = end;
        _entity.Interval = interval;
        return this;
    }

    internal ForecastIncomeBuilder WithRecurrenceRuleTypeId(Guid recurrenceRuleTypeId)
    {
        _entity.ForecastRecurrenceRuleTypeId = recurrenceRuleTypeId;
        return this;
    }

    internal ForecastIncomeBuilder WithIsActive(bool isActive)
    {
        _entity.IsActive = isActive;
        return this;
    }

    internal ForecastIncome Build() => _entity;
}

internal sealed class ForecastOccurrenceBuilder
{
    private readonly ForecastOccurrence _entity = new()
    {
        Id = Guid.NewGuid(),
        ForecastDefinitionId = Guid.NewGuid(),
        IsIncome = false,
        Description = "Occurrence",
        Amount = 1m,
        ExpectedDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
        ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
    };

    internal ForecastOccurrenceBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    internal ForecastOccurrenceBuilder WithForecastDefinitionId(Guid definitionId)
    {
        _entity.ForecastDefinitionId = definitionId;
        return this;
    }

    internal ForecastOccurrenceBuilder WithIsIncome(bool isIncome)
    {
        _entity.IsIncome = isIncome;
        return this;
    }

    internal ForecastOccurrenceBuilder WithDescription(string description)
    {
        _entity.Description = description;
        return this;
    }

    internal ForecastOccurrenceBuilder WithAmount(decimal amount)
    {
        _entity.Amount = amount;
        return this;
    }

    internal ForecastOccurrenceBuilder WithExpectedDate(DateOnly expectedDate)
    {
        _entity.ExpectedDate = expectedDate;
        return this;
    }

    internal ForecastOccurrenceBuilder WithPaymentCategoryId(Guid? paymentCategoryId)
    {
        _entity.PaymentCategoryId = paymentCategoryId;
        return this;
    }

    internal ForecastOccurrenceBuilder WithStatus(Guid statusId)
    {
        _entity.ForecastOccurrenceStatusId = statusId;
        return this;
    }

    internal ForecastOccurrenceBuilder WithValidatedAt(DateTime? validatedAt)
    {
        _entity.ValidatedAt = validatedAt;
        return this;
    }

    internal ForecastOccurrence Build() => _entity;
}
