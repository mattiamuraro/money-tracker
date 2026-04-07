using MoneyTracker.Api.Services;
using MoneyTracker.BusinessLogic.Services;

namespace MoneyTracker.ApiService.ExtensionMethods
{
    internal static class BuilderExtensionMethods
    {
        internal static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PaymentCategoryService>();
            builder.Services.AddScoped<ForecastService>();

        }
    }
}
