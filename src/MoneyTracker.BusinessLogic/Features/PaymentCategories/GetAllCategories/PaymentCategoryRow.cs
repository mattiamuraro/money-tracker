namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories
{
    public class PaymentCategoryRow
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
