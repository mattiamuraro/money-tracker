using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Models;

namespace MoneyTracker.BusinessLogic.Features.Payments.GetPayment;

internal static class GetPaymentRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<GetPaymentQueryHandler, GetPaymentQuery, PaginatedResponse<PaymentDto>>();

        return services;
    }
}