using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;

/// <summary>
/// Request DTO for updating an existing payment category
/// </summary>
public class UpdatePaymentCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(5)]
    public required string Code { get; set; }
}
