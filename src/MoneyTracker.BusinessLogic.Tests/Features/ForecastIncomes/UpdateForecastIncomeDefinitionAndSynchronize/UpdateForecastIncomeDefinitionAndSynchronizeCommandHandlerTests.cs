using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize;

public class UpdateForecastIncomeDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class UpdateHandlerStub : IHandler<UpdateForecastIncomeDefinitionCommand>
    {
        public bool WasCalled { get; private set; }
        public UpdateForecastIncomeDefinitionCommand? ReceivedRequest { get; private set; }

        public Task Handle(UpdateForecastIncomeDefinitionCommand request, CancellationToken cancellationToken)
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
    public async Task Handle_ShouldCallUpdateAndSynchronize()
    {
        var updateHandler = new UpdateHandlerStub();
        var syncHandler = new SyncHandlerStub();
        var sut = new UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler(updateHandler, syncHandler);

        var updateCommand = new UpdateForecastIncomeDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Salary",
            Amount = 2500m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        var command = new UpdateForecastIncomeDefinitionAndSynchronizeCommand { UpdateCommand = updateCommand };

        await sut.Handle(command, CancellationToken.None);

        Assert.True(updateHandler.WasCalled);
        Assert.Same(updateCommand, updateHandler.ReceivedRequest);
        Assert.True(syncHandler.WasCalled);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler(new UpdateHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WithNullUpdateCommand_ShouldThrowArgumentNullException()
    {
        var sut = new UpdateForecastIncomeDefinitionAndSynchronizeCommandHandler(new UpdateHandlerStub(), new SyncHandlerStub());
        var command = new UpdateForecastIncomeDefinitionAndSynchronizeCommand { UpdateCommand = null! };

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(command, CancellationToken.None));

        Assert.Equal("request.UpdateCommand", exception.ParamName);
    }
}
