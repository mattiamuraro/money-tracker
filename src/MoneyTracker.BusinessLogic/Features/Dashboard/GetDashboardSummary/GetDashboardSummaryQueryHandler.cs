using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(MoneyTrackerDbContext dbContext)
    : IHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(request.Year, request.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var monthStartDateOnly = DateOnly.FromDateTime(monthStart);
        var nextMonthStartDateOnly = DateOnly.FromDateTime(nextMonthStart);

        var paymentsQuery = dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Date >= monthStart && p.Date < nextMonthStart);

        var paymentsCountTask = paymentsQuery.CountAsync(cancellationToken);
        var paymentTotalTask = paymentsQuery.SumAsync(p => (decimal?)p.Amount, cancellationToken);
        var latestPaymentTask = paymentsQuery
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.CreatedAt)
            .Select(p => new { p.Description, p.Date })
            .FirstOrDefaultAsync(cancellationToken);

        var pendingForecastRowsTask = dbContext.ForecastOccurrences
            .AsNoTracking()
            .Where(o => o.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                && o.ExpectedDate >= monthStartDateOnly
                && o.ExpectedDate < nextMonthStartDateOnly)
            .Select(o => new { o.IsIncome, o.Amount, o.Description, o.ExpectedDate })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(paymentsCountTask, paymentTotalTask, latestPaymentTask, pendingForecastRowsTask);

        var pendingRows = pendingForecastRowsTask.Result;
        var forecastIncomeTotal = pendingRows.Where(x => x.IsIncome).Sum(x => x.Amount);
        var forecastExpenseTotal = pendingRows.Where(x => !x.IsIncome).Sum(x => x.Amount);
        var nextUpcomingExpense = pendingRows
            .Where(x => !x.IsIncome)
            .OrderBy(x => x.ExpectedDate)
            .ThenBy(x => x.Description)
            .FirstOrDefault();

        return new DashboardSummaryDto
        {
            PaymentsCount = paymentsCountTask.Result,
            PaymentTotal = paymentTotalTask.Result ?? 0m,
            LatestPaymentDescription = latestPaymentTask.Result?.Description,
            LatestPaymentDate = latestPaymentTask.Result?.Date,
            ForecastIncomeTotal = forecastIncomeTotal,
            ForecastExpenseTotal = forecastExpenseTotal,
            ForecastBalance = forecastIncomeTotal - forecastExpenseTotal,
            NextUpcomingExpenseDescription = nextUpcomingExpense?.Description,
        };
    }
}
