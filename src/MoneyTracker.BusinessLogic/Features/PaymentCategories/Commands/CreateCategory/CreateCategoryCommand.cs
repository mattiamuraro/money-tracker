using MediatR;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.CreateCategory
{
    /// <summary>
    /// Command to create a new payment category
    /// </summary>
    public class CreateCategoryCommand : IRequest<Guid>
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string CreatedBy { get; set; }
    }
}
