using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize;

public class CreateForecastIncomeDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class CreateHandlerStub : IHandler<CreateForecastIncomeDefinitionCommand, Guid>
    {
        public bool WasCalled { get; private set; }
        public CreateForecastIncomeDefinitionCommand? ReceivedRequest { get; private set; }
        public Guid ResultToReturn { get; set; } = Guid.NewGuid();

        public Task<Guid> Handle(CreateForecastIncomeDefinitionCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedRequest = request;
            return Task.FromResult(ResultToReturn);
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
    public async Task Handle_ShouldCallCreateAndSynchronize_AndReturnCreatedId()
    {
        var createHandler = new CreateHandlerStub { ResultToReturn = Guid.NewGuid() };
        var syncHandler = new SyncHandlerStub();
        var sut = new CreateForecastIncomeDefinitionAndSynchronizeCommandHandler(createHandler, syncHandler);

        var createCommand = new CreateForecastIncomeDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Salary",
            Amount = 2000m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1
        };

        var command = new CreateForecastIncomeDefinitionAndSynchronizeCommand { CreateCommand = createCommand };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.Equal(createHandler.ResultToReturn, result);
        Assert.True(createHandler.WasCalled);
        Assert.Same(createCommand, createHandler.ReceivedRequest);
        Assert.True(syncHandler.WasCalled);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new CreateForecastIncomeDefinitionAndSynchronizeCommandHandler(new CreateHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WithNullCreateCommand_ShouldThrowArgumentNullException()
    {
        var sut = new CreateForecastIncomeDefinitionAndSynchronizeCommandHandler(new CreateHandlerStub(), new SyncHandlerStub());
        var command = new CreateForecastIncomeDefinitionAndSynchronizeCommand { CreateCommand = null! };

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(command, CancellationToken.None));

        Assert.Equal("request.CreateCommand", exception.ParamName);
    }
}
