using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Contracts;
using MoneyTracker.Api.Endpoints.Forecasts.Contracts;
using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Features.Forecasts.Models;

namespace MoneyTracker.Api.Endpoints
{
    public static class ForecastEndpoints
    {
        internal static WebApplication AddForecastApis(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/forecasts")
                        .WithTags("Forecasts")
                        .RequireAuthorization();

            // GET all forecasts for a date range
            group.MapGet("/", static async ([FromServices] ForecastService forecastService, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken cancellationToken) =>
                    await forecastService.GetForecastRowAsync(startDate, endDate, cancellationToken))
                .WithName("GetForecasts")
                .WithDescription("Retrieves forecasts for a given date range")
                .Produces<List<ForecastRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions", static async ([FromServices] ForecastService forecastService, CancellationToken cancellationToken) =>
                    await forecastService.GetForecastDefinitionsAsync(cancellationToken))
                .WithName("GetForecastDefinitions")
                .WithDescription("Retrieves all forecast definitions")
                .Produces<List<ForecastDefinitionDto>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/definitions/{id:guid}", static async ([FromServices] ForecastService forecastService, Guid id, CancellationToken cancellationToken) =>
                    await forecastService.GetForecastDefinitionByIdAsync(id, cancellationToken))
                .WithName("GetForecastDefinitionById")
                .WithDescription("Retrieves a specific forecast definition by ID")
                .Produces<ForecastDefinitionDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapPost("/definitions", static async ([FromServices] ForecastService forecastService, CreateForecastRequest request, CancellationToken cancellationToken) =>
                    await forecastService.CreateForecastDefinitionAsync(request, cancellationToken))
                .WithName("CreateForecastDefinition")
                .WithDescription("Creates a new forecast definition")
                .Accepts<CreateForecastRequest>("application/json")
                .Produces<Guid>(StatusCodes.Status201Created)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapPut("/definitions/{id:guid}", static async ([FromServices] ForecastService forecastService, Guid id, UpdateForecastRequest request, CancellationToken cancellationToken) =>
                    await forecastService.UpdateForecastDefinitionAsync(id, request, cancellationToken))
                .WithName("UpdateForecastDefinition")
                .WithDescription("Updates an existing forecast definition")
                .Accepts<UpdateForecastRequest>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/definitions/{id:guid}", static async ([FromServices] ForecastService forecastService, Guid id, CancellationToken cancellationToken) =>
                    await forecastService.DeleteForecastDefinitionAsync(id, cancellationToken))
                .WithName("DeleteForecastDefinition")
                .WithDescription("Deletes a forecast definition")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapGet("/occurrences", static async ([FromServices] ForecastService forecastService, [AsParameters] ForecastOccurrenceQuery query, CancellationToken cancellationToken) =>
                    await forecastService.GetPendingOccurrencesAsync(query, cancellationToken))
                .WithName("GetForecastOccurrences")
                .WithDescription("Retrieves pending forecast occurrences for a month and type")
                .Produces<List<ForecastOccurrenceRow>>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/occurrences/{id:guid}", static async ([FromServices] ForecastService forecastService, Guid id, CancellationToken cancellationToken) =>
                    await forecastService.DiscardPendingOccurrenceAsync(id, cancellationToken))
                .WithName("DiscardForecastOccurrence")
                .WithDescription("Discards a pending forecast occurrence")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
