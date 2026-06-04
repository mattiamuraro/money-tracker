using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;

internal static class CreatePaymentRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreatePaymentCommandHandler, CreatePaymentCommand, Guid>();
        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();

        return services;
    }
}