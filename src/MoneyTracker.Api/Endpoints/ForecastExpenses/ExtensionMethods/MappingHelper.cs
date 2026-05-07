using MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;

namespace MoneyTracker.Api.Endpoints.ForecastExpenses.ExtensionMethods;

internal static class MappingHelper
{
    public static CreateForecastExpenseDefinitionCommand ToCreateForecastExpenseDefinitionCommand(this CreateForecastExpenseRequest entity)
    {
        return new CreateForecastExpenseDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
            PaymentCategoryId = entity.PaymentCategoryId ?? Guid.Empty,
        };
    }

    public static UpdateForecastExpenseDefinitionCommand ToUpdateForecastExpenseDefinitionCommand(this UpdateForecastExpenseRequest entity, Guid id)
    {
        return new UpdateForecastExpenseDefinitionCommand
        {
            Id = id,
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
            PaymentCategoryId = entity.PaymentCategoryId ?? Guid.Empty,
        };
    }

    public static ForecastExpenseDefinitionResponse ToForecastExpenseDefinitionResponse(this ForecastExpenseDefinitionDto entity)
    {
        return new ForecastExpenseDefinitionResponse
        {
            Id = entity.Id,
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
            PaymentCategoryId = entity.PaymentCategoryId,
            Category = entity.Category,
        };
    }

    public static ForecastExpenseRowResponse ToForecastExpenseRowResponse(this ForecastExpenseDto entity)
    {
        return new ForecastExpenseRowResponse
        {
            Id = entity.Id,
            ForecastDefinitionId = entity.ForecastDefinitionId,
            Description = entity.Description,
            Amount = entity.Amount,
            Date = entity.Date,
            PaymentCategoryId = entity.PaymentCategoryId,
            Category = entity.Category,
        };
    }

    public static ForecastExpenseOccurrenceResponse ToForecastExpenseOccurrenceResponse(this ForecastExpenseOccurrenceDto entity)
    {
        return new ForecastExpenseOccurrenceResponse
        {
            Id = entity.Id,
            ForecastDefinitionId = entity.ForecastDefinitionId,
            Description = entity.Description,
            Amount = entity.Amount,
            ExpectedDate = entity.ExpectedDate,
            PaymentCategoryId = entity.PaymentCategoryId,
            Category = entity.Category,
        };
    }

    public static GetPendingForecastExpenseOccurrencesQuery ToGetPendingForecastExpenseOccurrencesQuery(this ForecastExpenseOccurrencesQuery entity)
    {
        var (year, month) = entity.GetRequiredYearMonth();
        return new GetPendingForecastExpenseOccurrencesQuery(year, month);
    }
}
