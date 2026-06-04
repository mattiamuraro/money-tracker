using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize;

public class UpdateForecastExpenseDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class UpdateHandlerStub : IHandler<UpdateForecastExpenseDefinitionCommand>
    {
        public bool WasCalled { get; private set; }
        public UpdateForecastExpenseDefinitionCommand? ReceivedRequest { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }

        public Task Handle(UpdateForecastExpenseDefinitionCommand request, CancellationToken cancellationToken)
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
        public CancellationToken ReceivedToken { get; private set; }

        public Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSynchronize()
    {
        var updateHandler = new UpdateHandlerStub();
        var syncHandler = new SyncHandlerStub();
        var sut = new UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler(updateHandler, syncHandler);

        var updateCommand = new UpdateForecastExpenseDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1200m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid()
        };

        var command = new UpdateForecastExpenseDefinitionAndSynchronizeCommand { UpdateCommand = updateCommand };
        var token = new CancellationTokenSource().Token;

        await sut.Handle(command, token);

        Assert.True(updateHandler.WasCalled);
        Assert.Same(updateCommand, updateHandler.ReceivedRequest);
        Assert.Equal(token, updateHandler.ReceivedToken);

        Assert.True(syncHandler.WasCalled);
        Assert.Equal(token, syncHandler.ReceivedToken);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler(new UpdateHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WithNullUpdateCommand_ShouldThrowArgumentNullException()
    {
        var sut = new UpdateForecastExpenseDefinitionAndSynchronizeCommandHandler(new UpdateHandlerStub(), new SyncHandlerStub());
        var command = new UpdateForecastExpenseDefinitionAndSynchronizeCommand { UpdateCommand = null! };

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(command, CancellationToken.None));

        Assert.Equal("request.UpdateCommand", exception.ParamName);
    }
}
