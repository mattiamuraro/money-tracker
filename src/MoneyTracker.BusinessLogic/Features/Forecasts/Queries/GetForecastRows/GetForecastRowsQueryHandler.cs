using MoneyTracker.BusinessLogic.ExtensionMethods.Mapping;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;
using MediatR;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.Queries.GetForecastRows;

/// <summary>
/// Handler for the GetForecastRowsQuery query
/// </summary>
public class GetForecastRowsQueryHandler : IRequestHandler<GetForecastRowsQuery, List<ForecastRow>>
{
    private readonly MoneyTrackerDbContext _dbContext;

    public GetForecastRowsQueryHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForecastRow>> Handle(GetForecastRowsQuery request, CancellationToken cancellationToken)
    {
        var forecastRows = new List<ForecastRow>();

        var forecastExpenses = _dbContext.ForecastExpenses
            .Where(w => w.RecurrenceStart <= request.EndDate &&
                       (w.RecurrenceEnd == null || w.RecurrenceEnd >= request.StartDate))
            .ToList();

        var forecastIncomes = _dbContext.ForecastIncomes
            .Where(w => w.RecurrenceStart <= request.EndDate &&
                       (w.RecurrenceEnd == null || w.RecurrenceEnd >= request.StartDate))
            .ToList();

        forecastRows.AddRange(GetForecastRow(forecastExpenses, request.StartDate, request.EndDate));
        forecastRows.AddRange(GetForecastRow(forecastIncomes, request.StartDate, request.EndDate, true));

        return await Task.FromResult(forecastRows);
    }

    private List<ForecastRow> GetForecastRow<T>(List<T> forecasts, DateOnly startDate, DateOnly endDate, bool isIncome = false)
        where T : BaseForecast
    {
        var forecastRows = new List<ForecastRow>();

        foreach (var forecast in forecasts)
        {
            var recurrences = forecast.GetRecurrences(startDate, endDate);

            foreach (var recurrence in recurrences)
                forecastRows.Add(forecast.ToForecastRow(recurrence, isIncome));
        }

        return forecastRows;
    }
}
