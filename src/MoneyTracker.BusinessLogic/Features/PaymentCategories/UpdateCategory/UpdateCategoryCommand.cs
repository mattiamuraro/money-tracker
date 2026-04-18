namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory
{
    /// <summary>
    /// Command to update an existing payment category
    /// </summary>
    public class UpdateCategoryCommand
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
    }
}
