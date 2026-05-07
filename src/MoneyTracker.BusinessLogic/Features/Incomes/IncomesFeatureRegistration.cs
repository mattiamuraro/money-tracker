using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;

namespace MoneyTracker.BusinessLogic.Features.Incomes;

internal static class IncomesFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreateIncomeCommandHandler, CreateIncomeCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdateIncomeCommandHandler, UpdateIncomeCommand>();
        services.AddVoidHandlerWithLogging<DeleteIncomeCommandHandler, DeleteIncomeCommand>();
        services.AddHandlerWithLogging<GetIncomeQueryHandler, GetIncomeQuery, PaginatedResponse<IncomeRow>>();
        services.AddHandlerWithLogging<GetIncomeByIdQueryHandler, GetIncomeByIdQuery, IncomeRow>();
        services.AddScoped<IValidator<CreateIncomeCommand>, CreateIncomeCommandValidator>();
        services.AddScoped<IValidator<UpdateIncomeCommand>, UpdateIncomeCommandValidator>();

        return services;
    }
}
