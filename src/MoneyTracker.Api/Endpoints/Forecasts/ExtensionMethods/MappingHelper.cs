using Azure.Core;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace MoneyTracker.Api.Endpoints.Forecasts.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static DiscardPendingForecastOccurrenceCommand ToDiscardPendingForecastOccurrenceCommand(this Guid id)
        {
            return new DiscardPendingForecastOccurrenceCommand(id);
        }

        public static ForecastOccurrenceResponse ToForecastOccurrenceResponse(this ForecastOccurrenceRow entity)
        {
            return new ForecastOccurrenceResponse
            {
                Id = entity.Id,
                ForecastDefinitionId = entity.ForecastDefinitionId,
                Description = entity.Description,
                Amount = entity.Amount,
                ExpectedDate = entity.ExpectedDate,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId,
                Category = entity.Category
            };
        }

        public static ForecastDefinitionResponse ToForecastDefinitionResponse(this ForecastDefinitionRow entity)
        {
            return new ForecastDefinitionResponse
            {
                Id = entity.Id,
                ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
                Description = entity.Description,
                Amount = entity.Amount,
                RecurrenceStart = entity.RecurrenceStart,
                RecurrenceEnd = entity.RecurrenceEnd,
                Interval = entity.Interval,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId
            };
        }

        public static ForecastRowResponse ToForecastRowResponse(this ForecastRow entity)
        {
            return new ForecastRowResponse
            {
                Id = entity.Id,
                ForecastDefinitionId = entity.ForecastDefinitionId,
                Description = entity.Description,
                Amount = entity.Amount,
                Date = entity.Date,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId,
                Category = entity.Category
            };
        }

        public static GetForecastRowsQuery GetForecastRowsQuery(this DateOnly? startDate, DateOnly? endDate)
        {
            var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);
            var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

            return new GetForecastRowsQuery(start, end);
        }

        public static ForecastDefinitionResponse ToForecastDefinitionResponse(this ForecastDefinitionDto entity)
        {
            return new ForecastDefinitionResponse
            {
                Id = entity.Id,
                ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
                Description = entity.Description,
                Amount = entity.Amount,
                RecurrenceStart = entity.RecurrenceStart,
                RecurrenceEnd = entity.RecurrenceEnd,
                Interval = entity.Interval,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId
            };
        }

        public static CreateForecastDefinitionCommand ToCreateForecastDefinitionCommand(this CreateForecastRequest entity)
        {
            return new CreateForecastDefinitionCommand
            {
                ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
                Description = entity.Description,
                Amount = entity.Amount,
                RecurrenceStart = entity.RecurrenceStart,
                RecurrenceEnd = entity.RecurrenceEnd,
                Interval = entity.Interval,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId
            };
        }

        public static UpdateForecastDefinitionCommand ToUpdateForecastDefinitionCommand(this UpdateForecastRequest entity, Guid id)
        {
            return new UpdateForecastDefinitionCommand
            {
                Id = id,
                ForecastRecurrenceRuleTypeId = entity.ForecastRecurrenceRuleTypeId,
                Description = entity.Description,
                Amount = entity.Amount,
                RecurrenceStart = entity.RecurrenceStart,
                RecurrenceEnd = entity.RecurrenceEnd,
                Interval = entity.Interval,
                IsIncome = entity.IsIncome,
                PaymentCategoryId = entity.PaymentCategoryId
            };
        }

        public static DeleteForecastDefinitionCommand ToDeleteForecastDefinitionCommand(this Guid id)
        {
            return new DeleteForecastDefinitionCommand(id);
        }

        public static GetPendingForecastOccurrencesQuery ToGetPendingForecastOccurrencesQuery(this ForecastOccurrenceQuery entity)
        {
            var (year, month) = entity.GetRequiredYearMonth();
            return new GetPendingForecastOccurrencesQuery(year, month, entity.IsIncome);
        }
    }
}
