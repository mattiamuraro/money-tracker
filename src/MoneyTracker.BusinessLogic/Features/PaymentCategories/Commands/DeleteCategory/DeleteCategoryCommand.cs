using MediatR;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Commands.DeleteCategory
{
    /// <summary>
    /// Command to delete a payment category
    /// </summary>
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteCategoryCommand(Guid id)
        {
            Id = id;
        }
    }
}
