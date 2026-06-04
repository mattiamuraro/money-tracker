namespace MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;

public class PaymentCategoryResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
}
