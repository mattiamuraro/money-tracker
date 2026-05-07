using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Models;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

namespace MoneyTracker.BusinessLogic.Features.Payments;

internal static class PaymentsFeatureRegistration
{
    internal static IServiceCollection RegisterServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHandlerWithLogging<CreatePaymentCommandHandler, CreatePaymentCommand, Guid>();
        services.AddVoidHandlerWithLogging<UpdatePaymentCommandHandler, UpdatePaymentCommand>();
        services.AddVoidHandlerWithLogging<DeletePaymentCommandHandler, DeletePaymentCommand>();
        services.AddHandlerWithLogging<GetPaymentQueryHandler, GetPaymentQuery, PaginatedResponse<PaymentDto>>();
        services.AddHandlerWithLogging<GetPaymentByIdQueryHandler, GetPaymentByIdQuery, PaymentDto>();
        services.AddScoped<IValidator<CreatePaymentCommand>, CreatePaymentCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentCommand>, UpdatePaymentCommandValidator>();

        return services;
    }
}
