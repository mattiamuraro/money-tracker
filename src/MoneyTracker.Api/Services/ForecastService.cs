using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DeleteForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.DiscardPendingForecastOccurrence;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Forecasts.UpdateForecastDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitionById;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastDefinitions;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRows;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetPendingForecastOccurrences;

namespace MoneyTracker.Api.Services
{
    public class ForecastService
    {
        private readonly GetForecastRowsQueryHandler _getForecastRowsQueryHandler;
        private readonly GetPendingForecastOccurrencesQueryHandler _getPendingForecastOccurrencesQueryHandler;
        private readonly DiscardPendingForecastOccurrenceCommandHandler _discardPendingForecastOccurrenceCommandHandler;
        private readonly SynchronizeForecastOccurrencesCommandHandler _synchronizeForecastOccurrencesCommandHandler;
        private readonly GetForecastDefinitionsQueryHandler _getForecastDefinitionsQueryHandler;
        private readonly GetForecastDefinitionByIdQueryHandler _getForecastDefinitionByIdQueryHandler;
        private readonly CreateForecastDefinitionCommandHandler _createForecastDefinitionCommandHandler;
        private readonly UpdateForecastDefinitionCommandHandler _updateForecastDefinitionCommandHandler;
        private readonly DeleteForecastDefinitionCommandHandler _deleteForecastDefinitionCommandHandler;
        private readonly ILogger<ForecastService> _logger;

        public ForecastService(
            GetForecastRowsQueryHandler getForecastRowsQueryHandler,
            GetPendingForecastOccurrencesQueryHandler getPendingForecastOccurrencesQueryHandler,
            DiscardPendingForecastOccurrenceCommandHandler discardPendingForecastOccurrenceCommandHandler,
            SynchronizeForecastOccurrencesCommandHandler synchronizeForecastOccurrencesCommandHandler,
            GetForecastDefinitionsQueryHandler getForecastDefinitionsQueryHandler,
            GetForecastDefinitionByIdQueryHandler getForecastDefinitionByIdQueryHandler,
            CreateForecastDefinitionCommandHandler createForecastDefinitionCommandHandler,
            UpdateForecastDefinitionCommandHandler updateForecastDefinitionCommandHandler,
            DeleteForecastDefinitionCommandHandler deleteForecastDefinitionCommandHandler,
            ILogger<ForecastService> logger)
        {
            _getForecastRowsQueryHandler = getForecastRowsQueryHandler;
            _getPendingForecastOccurrencesQueryHandler = getPendingForecastOccurrencesQueryHandler;
            _discardPendingForecastOccurrenceCommandHandler = discardPendingForecastOccurrenceCommandHandler;
            _synchronizeForecastOccurrencesCommandHandler = synchronizeForecastOccurrencesCommandHandler;
            _getForecastDefinitionsQueryHandler = getForecastDefinitionsQueryHandler;
            _getForecastDefinitionByIdQueryHandler = getForecastDefinitionByIdQueryHandler;
            _createForecastDefinitionCommandHandler = createForecastDefinitionCommandHandler;
            _updateForecastDefinitionCommandHandler = updateForecastDefinitionCommandHandler;
            _deleteForecastDefinitionCommandHandler = deleteForecastDefinitionCommandHandler;
            _logger = logger;
        }

        public async Task<IResult> GetForecastRowAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
        {
            var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);
            var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

            if (end < start)
                return Results.BadRequest(new { message = "End date must be greater than or equal to start date." });

            if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 366)
                return Results.BadRequest(new { message = "Date range cannot exceed 366 days." });

            try
            {
                await SynchronizeOccurrencesAsync(cancellationToken);

                var query = new GetForecastRowsQuery(start, end);
                var forecasts = await _getForecastRowsQueryHandler.Handle(query, cancellationToken);

                return Results.Ok(forecasts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast rows");
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving forecast rows");
            }
        }

        public async Task<IResult> GetPendingOccurrencesAsync(ForecastOccurrenceQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var (year, month) = query.GetRequiredYearMonth();
                await SynchronizeOccurrencesAsync(cancellationToken);

                var queryRequest = new GetPendingForecastOccurrencesQuery(year, month, query.IsIncome);
                var items = await _getPendingForecastOccurrencesQueryHandler.Handle(queryRequest, cancellationToken);

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
                var command = new DiscardPendingForecastOccurrenceCommand(id);
                var discarded = await _discardPendingForecastOccurrenceCommandHandler.Handle(command, cancellationToken);

                if (!discarded)
                    return Results.NotFound();

                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
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
            var command = new SynchronizeForecastOccurrencesCommand();
            await _synchronizeForecastOccurrencesCommandHandler.Handle(command, cancellationToken);
        }

        public async Task<IResult> GetForecastDefinitionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetForecastDefinitionsQuery();
                var definitions = await _getForecastDefinitionsQueryHandler.Handle(query, cancellationToken);

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
                var query = new GetForecastDefinitionByIdQuery(id);
                var definition = await _getForecastDefinitionByIdQueryHandler.Handle(query, cancellationToken);

                return definition == null ? Results.NotFound() : Results.Ok(definition);
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
                var command = new CreateForecastDefinitionCommand
                {
                    ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                    Description = request.Description,
                    Amount = request.Amount,
                    RecurrenceStart = request.RecurrenceStart,
                    RecurrenceEnd = request.RecurrenceEnd,
                    Interval = request.Interval,
                    IsIncome = request.IsIncome,
                    PaymentCategoryId = request.PaymentCategoryId
                };

                var id = await _createForecastDefinitionCommandHandler.Handle(command, cancellationToken);
                await SynchronizeOccurrencesAsync(cancellationToken);

                return Results.Created($"/api/v1/forecasts/definitions/{id}", id);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
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
                var command = new UpdateForecastDefinitionCommand
                {
                    Id = id,
                    ForecastRecurrenceRuleTypeId = request.ForecastRecurrenceRuleTypeId,
                    Description = request.Description,
                    Amount = request.Amount,
                    RecurrenceStart = request.RecurrenceStart,
                    RecurrenceEnd = request.RecurrenceEnd,
                    Interval = request.Interval,
                    IsIncome = request.IsIncome,
                    PaymentCategoryId = request.PaymentCategoryId
                };

                var updated = await _updateForecastDefinitionCommandHandler.Handle(command, cancellationToken);
                if (!updated)
                    return Results.NotFound();

                await SynchronizeOccurrencesAsync(cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
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
                var command = new DeleteForecastDefinitionCommand(id);
                var deleted = await _deleteForecastDefinitionCommandHandler.Handle(command, cancellationToken);

                if (!deleted)
                    return Results.NotFound();

                await SynchronizeOccurrencesAsync(cancellationToken);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forecast definition with ID: {ForecastId}", id);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error deleting forecast definition");
            }
        }
    }
}
