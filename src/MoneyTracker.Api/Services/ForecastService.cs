using Microsoft.EntityFrameworkCore;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.BusinessLogic.ExtensionMethods.Mapping;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Services
{
    public class ForecastService
    {
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly ILogger<ForecastService> _logger;

        public ForecastService(MoneyTrackerDbContext dbContext, ILogger<ForecastService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public List<ForecastRow> GetForecastRow(DateOnly startDate, DateOnly endDate)
        {
            var forecastRows = new List<ForecastRow>();

            var forecastExpenses = _dbContext.ForecastExpenses
                .Include(x => x.ForecastRecurrenceRuleType)
                .Where(w => w.RecurrenceStart <= endDate && (w.RecurrenceEnd == null || w.RecurrenceEnd >= startDate))
                .ToList();
            var forecastIncomes = _dbContext.ForecastIncomes
                .Include(x => x.ForecastRecurrenceRuleType)
                .Where(w => w.RecurrenceStart <= endDate && (w.RecurrenceEnd == null || w.RecurrenceEnd >= startDate))
                .ToList();

            forecastRows.AddRange(GetForecastRow(forecastExpenses, startDate, endDate));
            forecastRows.AddRange(GetForecastRow(forecastIncomes, startDate, endDate, true));

            return forecastRows
                .OrderBy(x => x.Date)
                .ThenBy(x => x.Description)
                .ToList();
        }

        public async Task<IResult> GetForecastRecurrenceRuleTypesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var types = await _dbContext.ForecastRecurrenceRuleTypes
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new ForecastRecurrenceRuleTypeDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast recurrence rule types");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast recurrence rule types");
            }
        }

        public async Task<IResult> GetForecastDefinitionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var expenses = await _dbContext.ForecastExpenses
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .ToListAsync(cancellationToken);
                var incomes = await _dbContext.ForecastIncomes
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .ToListAsync(cancellationToken);

                var definitions = expenses.Select(x => ToDefinitionDto(x, false))
                    .Concat(incomes.Select(x => ToDefinitionDto(x, true)))
                    .OrderBy(x => x.RecurrenceStart)
                    .ThenBy(x => x.Description)
                    .ToList();

                return Results.Ok(definitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast definitions");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast definitions");
            }
        }

        public async Task<IResult> GetForecastDefinitionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var expense = await _dbContext.ForecastExpenses
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (expense != null)
                    return Results.Ok(ToDefinitionDto(expense, false));

                var income = await _dbContext.ForecastIncomes
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (income != null)
                    return Results.Ok(ToDefinitionDto(income, true));

                return Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast definition with ID: {ForecastId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast definition");
            }
        }

        public async Task<IResult> CreateForecastDefinitionAsync(CreateForecastRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.RecurrenceEnd.HasValue && request.RecurrenceEnd.Value < request.RecurrenceStart)
                    return Results.BadRequest(new { message = "Recurrence end date cannot be earlier than recurrence start date." });

                var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);
                var forecast = CreateForecastEntity(request, recurrenceRuleType);

                if (request.IsIncome)
                    _dbContext.ForecastIncomes.Add((ForecastIncome)forecast);
                else
                    _dbContext.ForecastExpenses.Add((ForecastExpense)forecast);

                await _dbContext.SaveChangesAsync(cancellationToken);
                return Results.Created($"/api/v1/forecasts/definitions/{forecast.Id}", forecast.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating forecast definition");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error creating forecast definition");
            }
        }

        public async Task<IResult> UpdateForecastDefinitionAsync(Guid id, UpdateForecastRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.RecurrenceEnd.HasValue && request.RecurrenceEnd.Value < request.RecurrenceStart)
                    return Results.BadRequest(new { message = "Recurrence end date cannot be earlier than recurrence start date." });

                var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);

                var expense = await _dbContext.ForecastExpenses
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (expense != null)
                {
                    await UpdateForecastAsync(id, expense, request, recurrenceRuleType, false, cancellationToken);
                    return Results.NoContent();
                }

                var income = await _dbContext.ForecastIncomes
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (income != null)
                {
                    await UpdateForecastAsync(id, income, request, recurrenceRuleType, true, cancellationToken);
                    return Results.NoContent();
                }

                return Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating forecast definition with ID: {ForecastId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error updating forecast definition");
            }
        }

        public async Task<IResult> DeleteForecastDefinitionAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var expense = await _dbContext.ForecastExpenses
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (expense != null)
                {
                    _dbContext.ForecastExpenses.Remove(expense);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return Results.NoContent();
                }

                var income = await _dbContext.ForecastIncomes
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (income != null)
                {
                    _dbContext.ForecastIncomes.Remove(income);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return Results.NoContent();
                }

                return Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forecast definition with ID: {ForecastId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting forecast definition");
            }
        }

        private async Task UpdateForecastAsync(Guid id, BaseForecast existingForecast, UpdateForecastRequest request, ForecastRecurrenceRuleType recurrenceRuleType, bool isIncome, CancellationToken cancellationToken)
        {
            if (isIncome == request.IsIncome)
            {
                ApplyForecastValues(existingForecast, request, recurrenceRuleType);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            BaseForecast replacement = request.IsIncome
                ? new ForecastIncome
                {
                    Description = existingForecast.Description
                }
                : new ForecastExpense
                {
                    Description = existingForecast.Description
                };

            replacement.Id = id;
            replacement.CreatedAt = existingForecast.CreatedAt;
            replacement.CreatedById = existingForecast.CreatedById;

            ApplyForecastValues(replacement, request, recurrenceRuleType);

            if (isIncome)
            {
                _dbContext.ForecastIncomes.Remove((ForecastIncome)existingForecast);
                _dbContext.ForecastExpenses.Add((ForecastExpense)replacement);
            }
            else
            {
                _dbContext.ForecastExpenses.Remove((ForecastExpense)existingForecast);
                _dbContext.ForecastIncomes.Add((ForecastIncome)replacement);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static void ApplyForecastValues(BaseForecast forecast, UpdateForecastRequest request, ForecastRecurrenceRuleType recurrenceRuleType)
        {
            forecast.Description = request.Description;
            forecast.Amount = request.Amount;
            forecast.RecurrenceStart = request.RecurrenceStart;
            forecast.RecurrenceEnd = request.RecurrenceEnd;
            forecast.Interval = request.Interval;
            forecast.ForecastRecurrenceRuleTypeId = recurrenceRuleType.Id;
            forecast.ForecastRecurrenceRuleType = recurrenceRuleType;
        }

        private static BaseForecast CreateForecastEntity(CreateForecastRequest request, ForecastRecurrenceRuleType recurrenceRuleType)
        {
            BaseForecast forecast = request.IsIncome
                ? new ForecastIncome
                {
                    Description = request.Description
                }
                : new ForecastExpense
                {
                    Description = request.Description
                };

            forecast.Id = Guid.NewGuid();
            forecast.Description = request.Description;
            forecast.Amount = request.Amount;
            forecast.RecurrenceStart = request.RecurrenceStart;
            forecast.RecurrenceEnd = request.RecurrenceEnd;
            forecast.Interval = request.Interval;
            forecast.ForecastRecurrenceRuleTypeId = recurrenceRuleType.Id;
            forecast.ForecastRecurrenceRuleType = recurrenceRuleType;

            return forecast;
        }

        private static ForecastDefinitionDto ToDefinitionDto(BaseForecast forecast, bool isIncome)
        {
            return new ForecastDefinitionDto
            {
                Id = forecast.Id,
                ForecastRecurrenceRuleTypeId = forecast.ForecastRecurrenceRuleTypeId,
                Description = forecast.Description,
                Amount = forecast.Amount,
                RecurrenceStart = forecast.RecurrenceStart,
                RecurrenceEnd = forecast.RecurrenceEnd,
                Interval = forecast.Interval ?? 1,
                IsIncome = isIncome
            };
        }

        private async Task<ForecastRecurrenceRuleType> GetRecurrenceRuleTypeAsync(Guid forecastRecurrenceRuleTypeId, CancellationToken cancellationToken)
        {
            var requestedId = forecastRecurrenceRuleTypeId;

            if (requestedId == Guid.Empty)
            {
                var dayRuleType = await _dbContext.ForecastRecurrenceRuleTypes
                    .FirstOrDefaultAsync(x => x.Code == ForecastRecurrenceRuleType.Day, cancellationToken);

                return dayRuleType
                    ?? throw new InvalidOperationException("Default recurrence rule type 'Day' was not found.");
            }

            var recurrenceRuleType = await _dbContext.ForecastRecurrenceRuleTypes
                .FirstOrDefaultAsync(x => x.Id == requestedId, cancellationToken);

            return recurrenceRuleType
                ?? throw new InvalidOperationException($"Forecast recurrence rule type '{requestedId}' was not found.");
        }

        private List<ForecastRow> GetForecastRow<T>(List<T> forecasts, DateOnly startDate, DateOnly endDate, bool isIncome = false) where T : BaseForecast
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
}
