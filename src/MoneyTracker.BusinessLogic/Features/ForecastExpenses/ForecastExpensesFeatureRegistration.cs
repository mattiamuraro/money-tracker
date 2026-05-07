using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses;

internal static class ForecastExpensesFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastExpenseDefinitionCommandHandler, CreateForecastExpenseDefinitionCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastExpenseDefinitionCommandHandler, UpdateForecastExpenseDefinitionCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastExpenseDefinitionCommandHandler, DeleteForecastExpenseDefinitionCommand>();
        services.AddHandlerWithLogging<CreateForecastExpenseDefinitionAndSynchronizeCommandHandler, CreateForecastExpenseDefinitionAndSynchronizeCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler, UpdateForecastExpenseDefinitionAndSynchronizeCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastExpenseDefinitionAndSynchronizeCommandHandler, DeleteForecastExpenseDefinitionAndSynchronizeCommand>();
        services.AddHandlerWithLogging<GetForecastExpenseDefinitionsQueryHandler, GetForecastExpenseDefinitionsQuery, List<ForecastExpenseDefinitionDto>>();
        services.AddHandlerWithLogging<GetForecastExpenseRowsQueryHandler, GetForecastExpenseRowsQuery, List<ForecastExpenseDto>>();
        services.AddHandlerWithLogging<GetPendingForecastExpenseOccurrencesQueryHandler, GetPendingForecastExpenseOccurrencesQuery, List<ForecastExpenseOccurrenceDto>>();
        services.AddVoidHandlerWithLogging<DiscardForecastExpenseOccurrenceCommandHandler, DiscardForecastExpenseOccurrenceCommand>();
        services.AddScoped<IValidator<CreateForecastExpenseDefinitionCommand>, CreateForecastExpenseDefinitionCommandValidator>();
        services.AddScoped<IValidator<UpdateForecastExpenseDefinitionCommand>, UpdateForecastExpenseDefinitionCommandValidator>();

        return services;
    }
}
