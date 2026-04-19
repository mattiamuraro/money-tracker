namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;

/// <summary>
/// DTO representing a payment category
/// </summary>
public class PaymentCategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
}
