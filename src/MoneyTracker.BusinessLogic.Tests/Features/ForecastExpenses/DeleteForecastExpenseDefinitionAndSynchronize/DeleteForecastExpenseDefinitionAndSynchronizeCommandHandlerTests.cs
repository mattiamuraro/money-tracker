using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize;

public class DeleteForecastExpenseDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class DeleteHandlerStub : IHandler<DeleteForecastExpenseDefinitionCommand>
    {
        public bool WasCalled { get; private set; }
        public DeleteForecastExpenseDefinitionCommand? ReceivedRequest { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }

        public Task Handle(DeleteForecastExpenseDefinitionCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedRequest = request;
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class SyncHandlerStub : IHandler<SynchronizeForecastOccurrencesCommand>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteWithId_AndSynchronize()
    {
        var deleteHandler = new DeleteHandlerStub();
        var syncHandler = new SyncHandlerStub();
        var sut = new DeleteForecastExpenseDefinitionAndSynchronizeCommandHandler(deleteHandler, syncHandler);

        var id = Guid.NewGuid();
        await sut.Handle(new DeleteForecastExpenseDefinitionAndSynchronizeCommand { Id = id }, CancellationToken.None);

        Assert.True(deleteHandler.WasCalled);
        Assert.NotNull(deleteHandler.ReceivedRequest);
        Assert.Equal(id, deleteHandler.ReceivedRequest!.Id);
        Assert.True(syncHandler.WasCalled);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new DeleteForecastExpenseDefinitionAndSynchronizeCommandHandler(new DeleteHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }
}
