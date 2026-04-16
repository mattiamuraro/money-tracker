using Microsoft.EntityFrameworkCore;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
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

        public async Task<List<ForecastRow>> GetForecastRowAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            await SynchronizeOccurrencesAsync(cancellationToken);

            return await _dbContext.ForecastOccurrences
                .AsNoTracking()
                .Include(x => x.PaymentCategory)
                .Where(x => x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId && x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
                .OrderBy(x => x.ExpectedDate)
                .ThenBy(x => x.Description)
                .Select(x => new ForecastRow
                {
                    Id = x.Id,
                    ForecastDefinitionId = x.ForecastDefinitionId,
                    Description = x.Description,
                    Amount = x.Amount,
                    Date = x.ExpectedDate,
                    IsIncome = x.IsIncome,
                    PaymentCategoryId = x.PaymentCategoryId,
                    Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IResult> GetPendingOccurrencesAsync(ForecastOccurrenceQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var (year, month) = query.GetRequiredYearMonth();
                await SynchronizeOccurrencesAsync(cancellationToken);

                var items = await _dbContext.ForecastOccurrences
                    .AsNoTracking()
                    .Include(x => x.PaymentCategory)
                    .Where(x => x.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId
                        && x.IsIncome == query.IsIncome
                        && x.ExpectedDate.Year == year
                        && x.ExpectedDate.Month == month)
                    .OrderBy(x => x.ExpectedDate)
                    .ThenBy(x => x.Description)
                    .Select(x => new ForecastOccurrenceRow
                    {
                        Id = x.Id,
                        ForecastDefinitionId = x.ForecastDefinitionId,
                        Description = x.Description,
                        Amount = x.Amount,
                        ExpectedDate = x.ExpectedDate,
                        IsIncome = x.IsIncome,
                        PaymentCategoryId = x.PaymentCategoryId,
                        Category = x.PaymentCategory != null ? x.PaymentCategory.Name : null
                    })
                    .ToListAsync(cancellationToken);

                return Results.Ok(items);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast occurrences");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast occurrences");
            }
        }

        public async Task<IResult> DiscardPendingOccurrenceAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var occurrence = await _dbContext.ForecastOccurrences
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (occurrence is null)
                    return Results.NotFound();

                if (occurrence.ForecastOccurrenceStatusId != ForecastOccurrenceStatus.PendingId)
                    return Results.BadRequest(new { message = "Only pending forecast occurrences can be discarded." });

                occurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.SkippedId;
                occurrence.ValidatedAt = null;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discarding forecast occurrence with ID: {ForecastOccurrenceId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error discarding forecast occurrence");
            }
        }

        public async Task SynchronizeOccurrencesAsync(CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetSynchronizationWindow();
            var expectedOccurrences = new Dictionary<OccurrenceKey, OccurrenceSeed>();

            var forecastExpenses = await _dbContext.ForecastExpenses
                .AsNoTracking()
                .Include(x => x.ForecastRecurrenceRuleType)
                .Include(x => x.PaymentCategory)
                .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
                .ToListAsync(cancellationToken);

            foreach (var forecast in forecastExpenses)
            {
                foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
                {
                    expectedOccurrences[new OccurrenceKey(forecast.Id, false, recurrence)] = new OccurrenceSeed(
                        forecast.Id,
                        false,
                        forecast.Description,
                        forecast.Amount,
                        recurrence,
                        forecast.PaymentCategoryId,
                        forecast.PaymentCategory.Name);
                }
            }

            var forecastIncomes = await _dbContext.ForecastIncomes
                .AsNoTracking()
                .Include(x => x.ForecastRecurrenceRuleType)
                .Where(x => x.IsActive && x.RecurrenceStart <= endDate && (x.RecurrenceEnd == null || x.RecurrenceEnd >= startDate))
                .ToListAsync(cancellationToken);

            foreach (var forecast in forecastIncomes)
            {
                foreach (var recurrence in forecast.GetRecurrences(startDate, endDate))
                {
                    expectedOccurrences[new OccurrenceKey(forecast.Id, true, recurrence)] = new OccurrenceSeed(
                        forecast.Id,
                        true,
                        forecast.Description,
                        forecast.Amount,
                        recurrence,
                        null,
                        null);
                }
            }

            var existingOccurrences = await _dbContext.ForecastOccurrences
                .Where(x => x.ExpectedDate >= startDate && x.ExpectedDate <= endDate)
                .ToListAsync(cancellationToken);

            var existingLookup = existingOccurrences.ToDictionary(
                x => new OccurrenceKey(x.ForecastDefinitionId, x.IsIncome, x.ExpectedDate),
                x => x);

            foreach (var existingOccurrence in existingOccurrences)
            {
                var key = new OccurrenceKey(existingOccurrence.ForecastDefinitionId, existingOccurrence.IsIncome, existingOccurrence.ExpectedDate);

                if (!expectedOccurrences.TryGetValue(key, out var seed))
                {
                    if (existingOccurrence.ForecastOccurrenceStatusId == ForecastOccurrenceStatus.PendingId)
                    {
                        existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.CancelledId;
                        existingOccurrence.ValidatedAt = null;
                    }

                    continue;
                }

                if (existingOccurrence.ForecastOccurrenceStatusId is var statusId && (statusId == ForecastOccurrenceStatus.PendingId || statusId == ForecastOccurrenceStatus.CancelledId))
                {
                    existingOccurrence.Description = seed.Description;
                    existingOccurrence.Amount = seed.Amount;
                    existingOccurrence.PaymentCategoryId = seed.PaymentCategoryId;
                    existingOccurrence.ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId;
                    existingOccurrence.ValidatedAt = null;
                }
            }

            foreach (var (key, seed) in expectedOccurrences)
            {
                if (existingLookup.ContainsKey(key))
                    continue;

                _dbContext.ForecastOccurrences.Add(new ForecastOccurrence
                {
                    Id = Guid.NewGuid(),
                    ForecastDefinitionId = seed.ForecastDefinitionId,
                    IsIncome = seed.IsIncome,
                    Description = seed.Description,
                    Amount = seed.Amount,
                    ExpectedDate = seed.ExpectedDate,
                    PaymentCategoryId = seed.PaymentCategoryId,
                    ForecastOccurrenceStatusId = ForecastOccurrenceStatus.PendingId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
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
                    .AsNoTracking()
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .Include(x => x.PaymentCategory)
                    .Where(x => x.IsActive)
                    .ToListAsync(cancellationToken);
                var incomes = await _dbContext.ForecastIncomes
                    .AsNoTracking()
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .Where(x => x.IsActive)
                    .ToListAsync(cancellationToken);

                var definitions = expenses.Select(ToDefinitionDto)
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
                    .AsNoTracking()
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .Include(x => x.PaymentCategory)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

                if (expense != null)
                    return Results.Ok(ToDefinitionDto(expense));

                var income = await _dbContext.ForecastIncomes
                    .AsNoTracking()
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

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
                var validationError = await ValidateForecastRequestAsync(request.IsIncome, request.PaymentCategoryId, request.RecurrenceStart, request.RecurrenceEnd, cancellationToken);
                if (validationError != null)
                    return validationError;

                var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);
                var forecast = CreateForecastEntity(request, recurrenceRuleType);

                if (request.IsIncome)
                    _dbContext.ForecastIncomes.Add((ForecastIncome)forecast);
                else
                    _dbContext.ForecastExpenses.Add((ForecastExpense)forecast);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await SynchronizeOccurrencesAsync(cancellationToken);

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
                var validationError = await ValidateForecastRequestAsync(request.IsIncome, request.PaymentCategoryId, request.RecurrenceStart, request.RecurrenceEnd, cancellationToken);
                if (validationError != null)
                    return validationError;

                var recurrenceRuleType = await GetRecurrenceRuleTypeAsync(request.ForecastRecurrenceRuleTypeId, cancellationToken);

                var expense = await _dbContext.ForecastExpenses
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

                if (expense != null)
                {
                    await UpdateForecastAsync(id, expense, request, recurrenceRuleType, false, cancellationToken);
                    await SynchronizeOccurrencesAsync(cancellationToken);
                    return Results.NoContent();
                }

                var income = await _dbContext.ForecastIncomes
                    .Include(x => x.ForecastRecurrenceRuleType)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

                if (income != null)
                {
                    await UpdateForecastAsync(id, income, request, recurrenceRuleType, true, cancellationToken);
                    await SynchronizeOccurrencesAsync(cancellationToken);
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
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

                if (expense != null)
                {
                    expense.IsActive = false;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await SynchronizeOccurrencesAsync(cancellationToken);
                    return Results.NoContent();
                }

                var income = await _dbContext.ForecastIncomes
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

                if (income != null)
                {
                    income.IsActive = false;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await SynchronizeOccurrencesAsync(cancellationToken);
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

        private async Task<IResult?> ValidateForecastRequestAsync(bool isIncome, Guid? paymentCategoryId, DateOnly recurrenceStart, DateOnly? recurrenceEnd, CancellationToken cancellationToken)
        {
            if (recurrenceEnd.HasValue && recurrenceEnd.Value < recurrenceStart)
                return Results.BadRequest(new { message = "Recurrence end date cannot be earlier than recurrence start date." });

            if (isIncome)
                return null;

            if (!paymentCategoryId.HasValue || paymentCategoryId == Guid.Empty)
                return Results.BadRequest(new { message = "Expense forecasts require a payment category." });

            var categoryExists = await _dbContext.PaymentCategories.AnyAsync(x => x.Id == paymentCategoryId.Value, cancellationToken);
            if (!categoryExists)
                return Results.BadRequest(new { message = "The requested payment category does not exist." });

            return null;
        }

        private async Task UpdateForecastAsync(Guid id, BaseForecast existingForecast, UpdateForecastRequest request, ForecastRecurrenceRuleType recurrenceRuleType, bool isIncome, CancellationToken cancellationToken)
        {
            if (isIncome == request.IsIncome)
            {
                ApplyForecastValues(existingForecast, request, recurrenceRuleType);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            existingForecast.IsActive = false;

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
            replacement.IsActive = true;

            ApplyForecastValues(replacement, request, recurrenceRuleType);

            if (isIncome)
            {
                _dbContext.ForecastExpenses.Add((ForecastExpense)replacement);
            }
            else
            {
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
            forecast.IsActive = true;

            if (forecast is ForecastExpense expense)
                expense.PaymentCategoryId = request.PaymentCategoryId ?? Guid.Empty;
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
                    Description = request.Description,
                    PaymentCategoryId = request.PaymentCategoryId ?? Guid.Empty
                };

            forecast.Id = Guid.NewGuid();
            forecast.Description = request.Description;
            forecast.Amount = request.Amount;
            forecast.RecurrenceStart = request.RecurrenceStart;
            forecast.RecurrenceEnd = request.RecurrenceEnd;
            forecast.Interval = request.Interval;
            forecast.ForecastRecurrenceRuleTypeId = recurrenceRuleType.Id;
            forecast.ForecastRecurrenceRuleType = recurrenceRuleType;
            forecast.IsActive = true;

            return forecast;
        }

        private static ForecastDefinitionDto ToDefinitionDto(ForecastExpense forecast)
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
                IsIncome = false,
                PaymentCategoryId = forecast.PaymentCategoryId,
                Category = forecast.PaymentCategory.Name
            };
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

        private static (DateOnly StartDate, DateOnly EndDate) GetSynchronizationWindow()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = new DateOnly(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }

        private readonly record struct OccurrenceKey(Guid ForecastDefinitionId, bool IsIncome, DateOnly ExpectedDate);
        private readonly record struct OccurrenceSeed(Guid ForecastDefinitionId, bool IsIncome, string Description, decimal Amount, DateOnly ExpectedDate, Guid? PaymentCategoryId, string? Category);
    }
}
