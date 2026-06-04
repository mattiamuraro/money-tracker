namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory
{
    /// <summary>
    /// Command to create a new payment category
    /// </summary>
    public class CreateCategoryCommand
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
    }
}
