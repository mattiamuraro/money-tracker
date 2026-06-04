using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

internal static class UpdateIncomeRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddVoidHandlerWithLogging<UpdateIncomeCommandHandler, UpdateIncomeCommand>();
        services.AddScoped<IValidator<UpdateIncomeCommand>, UpdateIncomeCommandValidator>();

        return services;
    }
}