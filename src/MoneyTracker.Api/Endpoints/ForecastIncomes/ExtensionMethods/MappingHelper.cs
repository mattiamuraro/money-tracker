using MoneyTracker.Api.Endpoints.ForecastIncomes.Contracts;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.Api.Endpoints.ForecastExpenses.Contracts;

namespace MoneyTracker.Api.Endpoints.ForecastIncomes.ExtensionMethods;

internal static class MappingHelper
{
    public static CreateForecastIncomeDefinitionCommand ToCreateForecastIncomeDefinitionCommand(this CreateForecastIncomeRequest entity)
    {
        return new CreateForecastIncomeDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
        };
    }

    public static UpdateForecastIncomeDefinitionCommand ToUpdateForecastIncomeDefinitionCommand(this UpdateForecastIncomeRequest entity, Guid id)
    {
        return new UpdateForecastIncomeDefinitionCommand
        {
            Id = id,
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
        };
    }

    public static ForecastIncomeDefinitionResponse ToForecastIncomeDefinitionResponse(this ForecastIncomeDefinitionRow entity)
    {
        return new ForecastIncomeDefinitionResponse
        {
            Id = entity.Id,
            ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
            Description = entity.Description,
            Amount = entity.Amount,
            RecurrenceStart = entity.RecurrenceStart,
            RecurrenceEnd = entity.RecurrenceEnd,
            Interval = entity.Interval,
        };
    }

    public static ForecastIncomeRowResponse ToForecastIncomeRowResponse(this ForecastIncomeRow entity)
    {
        return new ForecastIncomeRowResponse
        {
            Id = entity.Id,
            ForecastDefinitionId = entity.ForecastDefinitionId,
            Description = entity.Description,
            Amount = entity.Amount,
            Date = entity.Date,
        };
    }

    public static ForecastIncomeOccurrenceResponse ToForecastIncomeOccurrenceResponse(this ForecastIncomeOccurrenceRow entity)
    {
        return new ForecastIncomeOccurrenceResponse
        {
            Id = entity.Id,
            ForecastDefinitionId = entity.ForecastDefinitionId,
            Description = entity.Description,
            Amount = entity.Amount,
            ExpectedDate = entity.ExpectedDate,
        };
    }

    public static GetPendingForecastIncomeOccurrencesQuery ToGetPendingForecastIncomeOccurrencesQuery(this ForecastIncomeOccurencesQuery entity)
    {
        var (year, month) = entity.GetRequiredYearMonth();
        return new GetPendingForecastIncomeOccurrencesQuery(year, month);
    }
}
