namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.Models;

/// <summary>
/// DTO representing a payment category
/// </summary>
public class PaymentCategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public required string ModifiedBy { get; set; }
}
