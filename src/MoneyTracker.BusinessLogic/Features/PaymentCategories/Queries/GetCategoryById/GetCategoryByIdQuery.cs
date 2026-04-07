using MediatR;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Queries.GetCategoryById
{
    /// <summary>
    /// Query to retrieve a specific payment category by ID
    /// </summary>
    public class GetCategoryByIdQuery : IRequest<PaymentCategoryDto?>
    {
        public Guid Id { get; set; }

        public GetCategoryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
