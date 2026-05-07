using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MoneyTracker.BusinessLogic.Common.Extensions;

/// <summary>
/// Registers business logic services grouped by vertical slice.
/// </summary>
public static class BusinessLogicServiceCollectionExtensions
{
    private const string RegistrationMethodName = "RegisterServices";

    /// <summary>
    /// Registers all business logic handlers and validators.
    /// </summary>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var registrationMethod in GetRegistrationMethods())
            services = (IServiceCollection)registrationMethod.Invoke(null, [services])!;

        return services;
    }

    private static IEnumerable<MethodInfo> GetRegistrationMethods()
    {
        return typeof(BusinessLogicServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".Features.", StringComparison.Ordinal) == true)
            .Select(type => type.GetMethod(
                RegistrationMethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(IServiceCollection)],
                modifiers: null))
            .Where(method => method is not null && method.ReturnType == typeof(IServiceCollection))
            .OrderBy(method => method!.DeclaringType!.FullName)
            .Cast<MethodInfo>();
    }
}
