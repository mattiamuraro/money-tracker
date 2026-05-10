using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize;

public class DeleteForecastIncomeDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class DeleteHandlerStub : IHandler<DeleteForecastIncomeDefinitionCommand>
    {
        public bool WasCalled { get; private set; }
        public DeleteForecastIncomeDefinitionCommand? ReceivedRequest { get; private set; }

        public Task Handle(DeleteForecastIncomeDefinitionCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedRequest = request;
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
        var sut = new DeleteForecastIncomeDefinitionAndSynchronizeCommandHandler(deleteHandler, syncHandler);

        var id = Guid.NewGuid();
        await sut.Handle(new DeleteForecastIncomeDefinitionAndSynchronizeCommand { Id = id }, CancellationToken.None);

        Assert.True(deleteHandler.WasCalled);
        Assert.NotNull(deleteHandler.ReceivedRequest);
        Assert.Equal(id, deleteHandler.ReceivedRequest!.Id);
        Assert.True(syncHandler.WasCalled);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new DeleteForecastIncomeDefinitionAndSynchronizeCommandHandler(new DeleteHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }
}
