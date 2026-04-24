using MoneyTracker.Api.Endpoints.Incomes.Contracts;
using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.Api.Endpoints.Incomes.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static GetIncomeByIdQuery ToGetIncomeByIdQuery(this Guid id)
        {
            return new GetIncomeByIdQuery(id);
        }
        public static DeleteIncomeCommand ToDeleteIncomeCommand(this Guid id, string? occurrenceAction)
        {
            return new DeleteIncomeCommand
            {
                IncomeId = id,
                OccurrenceAction = occurrenceAction
            };
        }

        public static UpdateIncomeCommand ToUpdateIncomeCommand(this UpdateIncomeRequest request, Guid id)
        {
            return new UpdateIncomeCommand
            {
                IncomeId = id,
                Description = request.Description,
                Amount = request.Amount,
                Date = request.Date
            };
        }

        public static CreateIncomeCommand ToCreateIncomeCommand(this CreateIncomeRequest request, HttpContext httpContext)
        {
            var idempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();

            return new CreateIncomeCommand
            {
                Description = request.Description,
                ForecastOccurrenceId = request.ForecastOccurrenceId,
                Amount = request.Amount,
                Date = request.Date,
                CreatedById = httpContext.GetCurrentUserId(),
                IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? request.IdempotencyKey : idempotencyKey
            };
        }

        public static GetIncomeQuery ToGetIncomeQuery(this IncomeFilterQuery entity)
        {
            entity.Validate();
            var (year, month) = entity.GetRequiredYearMonth();

            return new GetIncomeQuery(
                    entity.DescriptionFilter,
                    entity  .MinAmount,
                    entity.MaxAmount,
                    year,
                    month,
                    entity.PageNumber ?? 1,
                    entity.PageSize ?? 20,
                    entity.SortBy,
                    entity.SortOrder);
        }

        public static PaginatedResponse<IncomeRowResponse> ToPaginatedResponse(this PaginatedResponse<IncomeRow> entity)
        {
            return new PaginatedResponse<IncomeRowResponse>
            {
                Items = entity.Items.Select(s => s.ToIncomeRowResponse()).ToList(),
                TotalItems = entity.TotalItems,
                PageNumber = entity.PageNumber,
                PageSize = entity.PageSize
            };
        }

        public static IncomeRowResponse ToIncomeRowResponse(this IncomeRow entity)
        {
            return new IncomeRowResponse
            {
                Id = entity.Id,
                Description = entity.Description,
                ForecastOccurrenceId = entity.ForecastOccurrenceId,
                ForecastExpectedDate = entity.ForecastExpectedDate,
                Amount = entity.Amount,
                Date = entity.Date
            };
        }

    }
}
