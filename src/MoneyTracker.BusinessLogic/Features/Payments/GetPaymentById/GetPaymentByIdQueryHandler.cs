using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;

public class GetPaymentByIdQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Description = p.Description,
                PaymentCategoryId = p.PaymentCategoryId,
                Category = p.PaymentCategory.Name,
                ForecastOccurrenceId = p.ForecastOccurrenceId,
                ForecastExpectedDate = p.ForecastOccurrence != null ? p.ForecastOccurrence.ExpectedDate : null,
                Amount = p.Amount,
                Date = p.Date,
                IsOneShot = p.IsOneShot
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            throw new EntityNotFoundException($"Payment with id {request.Id} not found");

        return result;
    }
}
