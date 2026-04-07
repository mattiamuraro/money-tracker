using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Api.Endpoints.PaymentCategories.Contracts;

/// <summary>
/// Request DTO for creating a new payment category
/// </summary>
public class CreatePaymentCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(5)]
    public required string Code { get; set; }
}
