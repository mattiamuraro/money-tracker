using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;

internal static class UpdateForecastIncomeDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<UpdateForecastIncomeDefinitionCommandHandler, UpdateForecastIncomeDefinitionCommand>();
        services.AddScoped<IValidator<UpdateForecastIncomeDefinitionCommand>, UpdateForecastIncomeDefinitionCommandValidator>();

        return services;
    }
}