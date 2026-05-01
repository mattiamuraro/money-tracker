using MoneyTracker.Api.Endpoints.Payments.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.Api.Endpoints.Payments.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static GetPaymentByIdQuery ToGetPaymentByIdQuery(this Guid id)
        {
            return new GetPaymentByIdQuery(id);
        }

        public static GetPaymentQuery ToGetPaymentQuery(this PaymentFilterQuery request)
        {
            request.Validate();
            var (year, month) = request.GetRequiredYearMonth();

            return new GetPaymentQuery(
                request.CategoryFilter,
                request.DescriptionFilter,
                request.CategoryId,
                request.MinAmount,
                request.MaxAmount,
                year,
                month,
                request.PageNumber ?? 1,
                request.PageSize ?? 20,
                request.SortBy,
                request.SortOrder);
        }

        public static CreatePaymentCommand ToCreatePaymentCommand(this CreatePaymentRequest request, HttpContext httpContext)
        {
            var idempotencyKey = httpContext.Request.Headers["X-Idempotency-Key"].ToString();
            return new CreatePaymentCommand
            {
                Description = request.Description,
                PaymentCategoryId = request.PaymentCategoryId,
                ForecastOccurrenceId = request.ForecastOccurrenceId,
                Amount = request.Amount,
                Date = request.Date,
                IsOneShot = request.IsOneShot,
                IdempotencyKey = string.IsNullOrEmpty(idempotencyKey) ? null : idempotencyKey
            };
        }

        public static PaginatedResponse<PaymentRowResponse> ToPaginatedResponse(this PaginatedResponse<PaymentRow> entity)
        {
            return new PaginatedResponse<PaymentRowResponse>
            {
                Items = entity.Items.Select(s => s.ToPaymentRowResponse()).ToList(),
                TotalItems = entity.TotalItems,
                PageNumber = entity.PageNumber,
                PageSize = entity.PageSize
            };
        }

        public static PaymentRowResponse ToPaymentRowResponse(this PaymentRow entity)
        {
            return new PaymentRowResponse
            {
                Id = entity.Id,
                Description = entity.Description,
                PaymentCategoryId = entity.PaymentCategoryId,
                Category = entity.Category,
                ForecastOccurrenceId = entity.ForecastOccurrenceId,
                ForecastExpectedDate = entity.ForecastExpectedDate,
                Amount = entity.Amount,
                Date = entity.Date,
                IsOneShot = entity.IsOneShot
            };
        }

        public static UpdatePaymentCommand ToUpdatePaymentCommand(this UpdatePaymentRequest request, Guid id)
        {
            return new UpdatePaymentCommand
            {
                PaymentId = id,
                Description = request.Description,
                PaymentCategoryId = request.PaymentCategoryId,
                Amount = request.Amount,
                Date = request.Date,
                IsOneShot = request.IsOneShot
            };
        }

        public static DeletePaymentCommand ToDeletePaymentCommand(this Guid id, string? occurrenceAction)
        {
            return new DeletePaymentCommand
            {
                PaymentId = id,
                OccurrenceAction = occurrenceAction
            };
        }
    }
}
