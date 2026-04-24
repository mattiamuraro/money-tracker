using MoneyTracker.BusinessLogic.Common.Services;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

public class SynchronizeForecastOccurrencesCommandHandler
{
    private readonly MoneyTrackerDbContext _dbContext;
    private readonly ForecastOccurrencesService _forecastOccurrencesService;

    public SynchronizeForecastOccurrencesCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
        _forecastOccurrencesService = new ForecastOccurrencesService(dbContext);
    }

    public async Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
    {
        await _forecastOccurrencesService.SynchronizeAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
