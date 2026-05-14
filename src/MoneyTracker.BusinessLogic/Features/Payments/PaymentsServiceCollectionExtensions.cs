using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.BusinessLogic.Features.Payments;

internal static class PaymentsServiceCollectionExtensions
{
    internal static IServiceCollection AddPaymentsFeatureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services = CreatePaymentRegistration.RegisterServices(services);
        services = DeletePaymentRegistration.RegisterServices(services);
        services = GetPaymentRegistration.RegisterServices(services);
        services = GetPaymentByIdRegistration.RegisterServices(services);
        services = UpdatePaymentRegistration.RegisterServices(services);

        return services;
    }
}
