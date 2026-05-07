using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;

public class GetIncomeByIdQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetIncomeByIdQuery, IncomeDto>
{
    public async Task<IncomeDto> Handle(GetIncomeByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Incomes
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new IncomeDto
            {
                Id = p.Id,
                Description = p.Description,
                ForecastOccurrenceId = p.ForecastOccurrenceId,
                ForecastExpectedDate = p.ForecastOccurrence != null ? p.ForecastOccurrence.ExpectedDate : null,
                Amount = p.Amount,
                Date = p.Date
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (result == null)
            throw new EntityNotFoundException($"Income with id {request.Id} not found");

        return result;
    }
}

