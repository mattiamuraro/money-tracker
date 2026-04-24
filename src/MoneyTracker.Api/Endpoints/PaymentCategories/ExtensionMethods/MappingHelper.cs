using MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;

namespace MoneyTracker.Api.Endpoints.PaymentCategories.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static GetCategoryByIdQuery ToGetCategoryByIdQuery(this Guid id)
        {
            return new GetCategoryByIdQuery(id);
        }

        public static DeleteCategoryCommand ToDeleteCategoryCommand(this Guid id)
        {
            return new DeleteCategoryCommand(id);
        }

        public static IEnumerable<PaymentCategoryResponse>? ToPaymentCategoryResponses(this IEnumerable<PaymentCategoryRow>? entities)
        {
            return entities?.Select(c => c.ToPaymentCategoryResponse());
        }

        public static PaymentCategoryResponse ToPaymentCategoryResponse(this PaymentCategoryRow entity)
        {
            return new PaymentCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code
            };
        }

        public static PaymentCategoryResponse ToPaymentCategoryResponse(this PaymentCategoryDto entity)
        {
            return new PaymentCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code
            };
        }

        public static CreateCategoryCommand ToCreateCategoryCommand(this CreatePaymentCategoryRequest entity)
        {
            return new CreateCategoryCommand
            {
                Name = entity.Name,
                Code = entity.Code
            };
        }

        public static UpdateCategoryCommand ToUpdateCategoryCommand(this UpdatePaymentCategoryRequest entity, Guid id)
        {
            return new UpdateCategoryCommand
            {
                Id = id,
                Name = entity.Name,
                Code = entity.Code
            };

        }
    }
}
