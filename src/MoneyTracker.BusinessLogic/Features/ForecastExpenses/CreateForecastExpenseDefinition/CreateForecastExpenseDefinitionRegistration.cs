using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;

internal static class CreateForecastExpenseDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastExpenseDefinitionCommandHandler, CreateForecastExpenseDefinitionCommand, Guid>();
        services.AddScoped<IValidator<CreateForecastExpenseDefinitionCommand>, CreateForecastExpenseDefinitionCommandValidator>();

        return services;
    }
}