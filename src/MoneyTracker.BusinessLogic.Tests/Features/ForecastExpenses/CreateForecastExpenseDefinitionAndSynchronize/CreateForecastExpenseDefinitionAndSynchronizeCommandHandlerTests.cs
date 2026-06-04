using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize;

public class CreateForecastExpenseDefinitionAndSynchronizeCommandHandlerTests
{
    private sealed class CreateHandlerStub : IHandler<CreateForecastExpenseDefinitionCommand, Guid>
    {
        public bool WasCalled { get; private set; }
        public CreateForecastExpenseDefinitionCommand? ReceivedRequest { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }
        public Guid ResultToReturn { get; set; } = Guid.NewGuid();

        public Task<Guid> Handle(CreateForecastExpenseDefinitionCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedRequest = request;
            ReceivedToken = cancellationToken;
            return Task.FromResult(ResultToReturn);
        }
    }

    private sealed class SyncHandlerStub : IHandler<SynchronizeForecastOccurrencesCommand>
    {
        public bool WasCalled { get; private set; }
        public SynchronizeForecastOccurrencesCommand? ReceivedRequest { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }

        public Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedRequest = request;
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_ShouldCallCreateAndSynchronize_AndReturnCreatedId()
    {
        var createHandler = new CreateHandlerStub { ResultToReturn = Guid.NewGuid() };
        var syncHandler = new SyncHandlerStub();
        var sut = new CreateForecastExpenseDefinitionAndSynchronizeCommandHandler(createHandler, syncHandler);

        var createCommand = new CreateForecastExpenseDefinitionCommand
        {
            ForecastRecurrenceRuleTypeId = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1100m,
            RecurrenceStart = DateOnly.FromDateTime(DateTime.Today),
            Interval = 1,
            PaymentCategoryId = Guid.NewGuid()
        };

        var command = new CreateForecastExpenseDefinitionAndSynchronizeCommand { CreateCommand = createCommand };
        var token = new CancellationTokenSource().Token;

        var result = await sut.Handle(command, token);

        Assert.Equal(createHandler.ResultToReturn, result);
        Assert.True(createHandler.WasCalled);
        Assert.Same(createCommand, createHandler.ReceivedRequest);
        Assert.Equal(token, createHandler.ReceivedToken);

        Assert.True(syncHandler.WasCalled);
        Assert.NotNull(syncHandler.ReceivedRequest);
        Assert.Equal(token, syncHandler.ReceivedToken);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var sut = new CreateForecastExpenseDefinitionAndSynchronizeCommandHandler(new CreateHandlerStub(), new SyncHandlerStub());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(request: null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WithNullCreateCommand_ShouldThrowArgumentNullException()
    {
        var sut = new CreateForecastExpenseDefinitionAndSynchronizeCommandHandler(new CreateHandlerStub(), new SyncHandlerStub());
        var command = new CreateForecastExpenseDefinitionAndSynchronizeCommand { CreateCommand = null! };

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Handle(command, CancellationToken.None));

        Assert.Equal("request.CreateCommand", exception.ParamName);
    }
}
