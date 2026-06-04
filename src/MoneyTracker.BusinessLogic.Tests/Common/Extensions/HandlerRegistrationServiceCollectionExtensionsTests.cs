using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Handlers;

namespace MoneyTracker.BusinessLogic.Tests.Common.Extensions;

public class HandlerRegistrationServiceCollectionExtensionsTests
{
    private sealed class Request;

    private sealed class ResultHandler : IHandler<Request, string>
    {
        public Task<string> Handle(Request request, CancellationToken cancellationToken) => Task.FromResult("ok");
    }

    private sealed class VoidHandler : IHandler<Request>
    {
        public Task Handle(Request request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public void AddHandlerWithLogging_ShouldRegisterDecoratorAndConcreteHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var extensionType = typeof(BusinessLogicServiceCollectionExtensions).Assembly
            .GetType("MoneyTracker.BusinessLogic.Common.Extensions.HandlerRegistrationServiceCollectionExtensions");
        Assert.NotNull(extensionType);

        var method = extensionType!.GetMethod("AddHandlerWithLogging", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);

        var genericMethod = method!.MakeGenericMethod(typeof(ResultHandler), typeof(Request), typeof(string));
        var result = genericMethod.Invoke(null, [services]);

        Assert.Same(services, result);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ResultHandler>());
        Assert.NotNull(provider.GetService<IHandler<Request, string>>());
    }

    [Fact]
    public void AddVoidHandlerWithLogging_ShouldRegisterDecoratorAndConcreteHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var extensionType = typeof(BusinessLogicServiceCollectionExtensions).Assembly
            .GetType("MoneyTracker.BusinessLogic.Common.Extensions.HandlerRegistrationServiceCollectionExtensions");
        Assert.NotNull(extensionType);

        var method = extensionType!.GetMethod("AddVoidHandlerWithLogging", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);

        var genericMethod = method!.MakeGenericMethod(typeof(VoidHandler), typeof(Request));
        var result = genericMethod.Invoke(null, [services]);

        Assert.Same(services, result);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<VoidHandler>());
        Assert.NotNull(provider.GetService<IHandler<Request>>());
    }
}
