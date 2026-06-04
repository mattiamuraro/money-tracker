using Microsoft.Extensions.Logging;
using MoneyTracker.BusinessLogic.Common.Handlers;

namespace MoneyTracker.BusinessLogic.Tests.Common.Handlers;

public class LoggingHandlerDecoratorTests
{
    private sealed class TestRequest;

    private sealed class ResultHandlerStub(Func<CancellationToken, Task<string>> handle) : IHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken) => handle(cancellationToken);
    }

    private sealed class VoidHandlerStub(Func<CancellationToken, Task> handle) : IHandler<TestRequest>
    {
        public Task Handle(TestRequest request, CancellationToken cancellationToken) => handle(cancellationToken);
    }

    [Fact]
    public async Task Handle_ResultDecorator_ShouldReturnInnerResult()
    {
        var inner = new ResultHandlerStub(_ => Task.FromResult("ok"));
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<LoggingHandlerDecorator<TestRequest, string>>();
        var sut = new LoggingHandlerDecorator<TestRequest, string>(inner, logger);

        var result = await sut.Handle(new TestRequest(), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ResultDecorator_ShouldRethrowExceptions()
    {
        var inner = new ResultHandlerStub(_ => throw new InvalidOperationException("boom"));
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<LoggingHandlerDecorator<TestRequest, string>>();
        var sut = new LoggingHandlerDecorator<TestRequest, string>(inner, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(new TestRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_VoidDecorator_ShouldComplete_WhenInnerCompletes()
    {
        var inner = new VoidHandlerStub(_ => Task.CompletedTask);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<LoggingHandlerDecorator<TestRequest>>();
        var sut = new LoggingHandlerDecorator<TestRequest>(inner, logger);

        await sut.Handle(new TestRequest(), CancellationToken.None);

        Assert.True(true);
    }

    [Fact]
    public async Task Handle_VoidDecorator_ShouldRethrowCanceledException_WhenTokenIsCanceled()
    {
        var inner = new VoidHandlerStub(ct => throw new OperationCanceledException(ct));
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<LoggingHandlerDecorator<TestRequest>>();
        var sut = new LoggingHandlerDecorator<TestRequest>(inner, logger);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(new TestRequest(), cts.Token));
    }
}
