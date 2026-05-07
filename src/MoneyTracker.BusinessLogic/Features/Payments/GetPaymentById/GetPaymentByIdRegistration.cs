using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

namespace MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;

internal static class GetPaymentByIdRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetPaymentByIdQueryHandler, GetPaymentByIdQuery, PaymentDto>();

        return services;
    }
}