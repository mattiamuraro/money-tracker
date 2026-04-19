namespace MoneyTracker.Api.Endpoints.Payments.Contracts;

public class UpdatePaymentRequest
{
    public string? Description { get; set; }
    public Guid? PaymentCategoryId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public bool? IsOneShot { get; set; }
}
