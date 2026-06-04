using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;

internal static class CreateIncomeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateIncomeCommandHandler, CreateIncomeCommand, Guid>();
        services.AddScoped<IValidator<CreateIncomeCommand>, CreateIncomeCommandValidator>();

        return services;
    }
}