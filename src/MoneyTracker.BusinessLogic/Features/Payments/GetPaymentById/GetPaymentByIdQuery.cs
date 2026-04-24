namespace MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;

public class GetPaymentByIdQuery
{
    public Guid Id { get; set; }

    public GetPaymentByIdQuery(Guid id)
    {
        Id = id;
    }
}
