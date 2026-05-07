using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;

internal static class UpdateForecastExpenseDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<UpdateForecastExpenseDefinitionCommandHandler, UpdateForecastExpenseDefinitionCommand>();
        services.AddScoped<IValidator<UpdateForecastExpenseDefinitionCommand>, UpdateForecastExpenseDefinitionCommandValidator>();

        return services;
    }
}