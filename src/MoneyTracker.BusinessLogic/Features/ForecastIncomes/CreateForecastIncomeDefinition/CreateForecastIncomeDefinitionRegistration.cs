using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;

internal static class CreateForecastIncomeDefinitionRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateForecastIncomeDefinitionCommandHandler, CreateForecastIncomeDefinitionCommand, Guid>();
        services.AddScoped<IValidator<CreateForecastIncomeDefinitionCommand>, CreateForecastIncomeDefinitionCommandValidator>();

        return services;
    }
}