using MediatR;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.UpdateCategory
{
    /// <summary>
    /// Command to update an existing payment category
    /// </summary>
    public class UpdateCategoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string ModifiedBy { get; set; }
    }
}
