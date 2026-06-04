namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;

public class GetIncomeByIdQuery
{
    public Guid Id { get; set; }

    public GetIncomeByIdQuery(Guid id)
    {
        Id = id;
    }
}
