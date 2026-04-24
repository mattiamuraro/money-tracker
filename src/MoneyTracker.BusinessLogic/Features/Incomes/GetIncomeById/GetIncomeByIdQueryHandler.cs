
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;

public class GetIncomeByIdQueryHandler
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetIncomeByIdQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IncomeRow> Handle(GetIncomeByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Incomes
            .Where(p => p.Id == request.Id)
            .Select(p => new IncomeRow
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

