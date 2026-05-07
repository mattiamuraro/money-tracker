using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes;

internal static class ForecastIncomesFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastIncomeDefinitionCommandHandler, CreateForecastIncomeDefinitionCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastIncomeDefinitionCommandHandler, UpdateForecastIncomeDefinitionCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastIncomeDefinitionCommandHandler, DeleteForecastIncomeDefinitionCommand>();
        services.AddHandlerWithLogging<CreateForecastIncomeDefinitionAndSynchronizeCommandHandler, CreateForecastIncomeDefinitionAndSynchronizeCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler, UpdateForecastIncomeDefinitionAndSynchronizeCommand>();
        services.AddVoidHandlerWithLogging<DeleteForecastIncomeDefinitionAndSynchronizeCommandHandler, DeleteForecastIncomeDefinitionAndSynchronizeCommand>();
        services.AddHandlerWithLogging<GetForecastIncomeDefinitionsQueryHandler, GetForecastIncomeDefinitionsQuery, List<ForecastIncomeDefinitionDto>>();
        services.AddHandlerWithLogging<GetForecastIncomeRowsQueryHandler, GetForecastIncomeRowsQuery, List<ForecastIncomeDto>>();
        services.AddHandlerWithLogging<GetPendingForecastIncomeOccurrencesQueryHandler, GetPendingForecastIncomeOccurrencesQuery, List<ForecastIncomeOccurrenceDto>>();
        services.AddVoidHandlerWithLogging<DiscardForecastIncomeOccurrenceCommandHandler, DiscardForecastIncomeOccurrenceCommand>();
        services.AddScoped<IValidator<CreateForecastIncomeDefinitionCommand>, CreateForecastIncomeDefinitionCommandValidator>();
        services.AddScoped<IValidator<UpdateForecastIncomeDefinitionCommand>, UpdateForecastIncomeDefinitionCommandValidator>();

        return services;
    }
}
