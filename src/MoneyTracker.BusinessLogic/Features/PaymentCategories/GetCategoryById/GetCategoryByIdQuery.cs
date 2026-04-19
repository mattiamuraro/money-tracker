namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById
{
    /// <summary>
    /// Query to retrieve a specific payment category by ID
    /// </summary>
    public class GetCategoryByIdQuery
    {
        public Guid Id { get; set; }

        public GetCategoryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
