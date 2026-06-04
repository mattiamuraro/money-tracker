using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsOptions = Microsoft.Extensions.Options.Options;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.ReconciliationWorker;
using MoneyTracker.ReconciliationWorker.Incidents;
using MoneyTracker.ReconciliationWorker.Options;

namespace MoneyTracker.ReconciliationWorker.UnitTests;

public class ForecastOccurrenceReconciliationServiceTests
{
    // ─────────────────────────────────────────────────────── Fake helpers ───

    private sealed class FakeLogger : ILogger<ForecastOccurrenceReconciliationService>
    {
        public record LogEntry(LogLevel Level, EventId EventId, Exception? Exception, string Message);
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add(new LogEntry(logLevel, eventId, exception, formatter(state, exception)));
    }

    private sealed class FakeIncidentNotifier : IWorkerIncidentNotifier
    {
        public List<(int ConsecutiveFailures, TimeSpan FailureDuration, TimeSpan NextRetryDelay)> Notifications { get; } = [];
        public bool ShouldThrow { get; set; }

        public Task NotifyEscalationAsync(int consecutiveFailures, TimeSpan failureDuration, TimeSpan nextRetryDelay, CancellationToken cancellationToken)
        {
            if (ShouldThrow)
                throw new InvalidOperationException("Notification failed.");
            Notifications.Add((consecutiveFailures, failureDuration, nextRetryDelay));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHandler : IHandler<SynchronizeForecastOccurrencesCommand>
    {
        private readonly Queue<Func<CancellationToken, Task>> _results;

        public FakeHandler(params Func<CancellationToken, Task>[] results)
            => _results = new Queue<Func<CancellationToken, Task>>(results);

        public int CallCount { get; private set; }

        public Task Handle(SynchronizeForecastOccurrencesCommand request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (_results.TryDequeue(out var action))
                return action(cancellationToken);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeServiceScope(IServiceProvider provider) : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; } = provider;
        public void Dispose() { }
    }

    private sealed class FakeServiceScopeFactory(IServiceProvider scopeProvider) : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => new FakeServiceScope(scopeProvider);
    }

    private sealed class FakeRootServiceProvider(IServiceScopeFactory scopeFactory) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IServiceScopeFactory) ? scopeFactory : null;
    }

    // Subclass that skips all real Task.Delay calls so tests run instantly.
    private sealed class TestableReconciliationService(
        IServiceProvider serviceProvider,
        ILogger<ForecastOccurrenceReconciliationService> logger,
        Microsoft.Extensions.Options.IOptions<WorkerResilienceOptions> options,
        IWorkerIncidentNotifier notifier)
        : ForecastOccurrenceReconciliationService(serviceProvider, logger, options, notifier)
    {
        protected override Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static IServiceProvider BuildServiceProvider(IHandler<SynchronizeForecastOccurrencesCommand> handler)
    {
        var scopeServices = new ServiceCollection();
        scopeServices.AddSingleton(handler);
        var scopeProvider = scopeServices.BuildServiceProvider();
        var factory = new FakeServiceScopeFactory(scopeProvider);
        return new FakeRootServiceProvider(factory);
    }

    private static ForecastOccurrenceReconciliationService CreateService(
        IHandler<SynchronizeForecastOccurrencesCommand>? handler = null,
        WorkerResilienceOptions? options = null,
        FakeLogger? logger = null,
        FakeIncidentNotifier? notifier = null)
    {
        handler ??= new FakeHandler();
        var rootProvider = BuildServiceProvider(handler);
        var resolvedOptions = options ?? new WorkerResilienceOptions
        {
            MaxConsecutiveFailures = 3,
            EscalationWindowMinutes = 1,
            EscalationCooldownMinutes = 1
        };
        return new TestableReconciliationService(
            rootProvider,
            logger ?? new FakeLogger(),
            MsOptions.Create(resolvedOptions),
            notifier ?? new FakeIncidentNotifier());
    }

    private static async Task<Exception?> RunUntilCancelledAsync(
        ForecastOccurrenceReconciliationService service,
        CancellationToken cancellationToken)
    {
        await service.StartAsync(cancellationToken);

        if (service.ExecuteTask is null)
            return null;

        try
        {
            await service.ExecuteTask;
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // ─────────────────────────────────────────────────── Constructor tests ───

    [Fact]
    public void Constructor_Should_Initialize_Service()
    {
        var service = CreateService();

        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceProvider_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ForecastOccurrenceReconciliationService(
                null!,
                new FakeLogger(),
                MsOptions.Create(new WorkerResilienceOptions()),
                new FakeIncidentNotifier()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var provider = BuildServiceProvider(new FakeHandler());

        Assert.Throws<ArgumentNullException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                null!,
                MsOptions.Create(new WorkerResilienceOptions()),
                new FakeIncidentNotifier()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Options_Is_Null()
    {
        var provider = BuildServiceProvider(new FakeHandler());

        Assert.Throws<ArgumentNullException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                new FakeLogger(),
                null!,
                new FakeIncidentNotifier()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_IncidentNotifier_Is_Null()
    {
        var provider = BuildServiceProvider(new FakeHandler());

        Assert.Throws<ArgumentNullException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                new FakeLogger(),
                MsOptions.Create(new WorkerResilienceOptions()),
                null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_MaxConsecutiveFailures_Is_Less_Than_One(int value)
    {
        var provider = BuildServiceProvider(new FakeHandler());
        var opts = new WorkerResilienceOptions { MaxConsecutiveFailures = value };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                new FakeLogger(),
                MsOptions.Create(opts),
                new FakeIncidentNotifier()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_EscalationWindowMinutes_Is_Less_Than_One(int value)
    {
        var provider = BuildServiceProvider(new FakeHandler());
        var opts = new WorkerResilienceOptions { EscalationWindowMinutes = value };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                new FakeLogger(),
                MsOptions.Create(opts),
                new FakeIncidentNotifier()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_EscalationCooldownMinutes_Is_Less_Than_One(int value)
    {
        var provider = BuildServiceProvider(new FakeHandler());
        var opts = new WorkerResilienceOptions { EscalationCooldownMinutes = value };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ForecastOccurrenceReconciliationService(
                provider,
                new FakeLogger(),
                MsOptions.Create(opts),
                new FakeIncidentNotifier()));
    }

    // ──────────────────────────────────────────── Successful reconciliation ──

    [Fact]
    public async Task ExecuteAsync_Should_Call_Handler_And_Log_Success()
    {
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var callCount = 0;
        var handler = new FakeHandler(async ct =>
        {
            callCount++;
            await cts.CancelAsync();
        });

        var service = CreateService(handler: handler, logger: logger);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Equal(1, callCount);
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("succeeded"));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Started_Before_Executing()
    {
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var handler = new FakeHandler(async ct =>
        {
            await cts.CancelAsync();
        });

        var service = CreateService(handler: handler, logger: logger);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("started"));
    }

    // ──────────────────────────────────────────────────── Failure / retry ───

    [Fact]
    public async Task ExecuteAsync_Should_Log_Error_When_Handler_Throws()
    {
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var callCount = 0;
        var handler = new FakeHandler(
            _ =>
            {
                callCount++;
                throw new InvalidOperationException("sync fail");
            },
            async ct =>
            {
                callCount++;
                await cts.CancelAsync();
            });

        var service = CreateService(handler: handler, logger: logger);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Equal(2, callCount);
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Error &&
            e.Message.Contains("Error reconciling"));
    }

    // ─────────────────────────────────────────────────────── Cancellation ───

    [Fact]
    public async Task ExecuteAsync_Should_Log_Cancellation_When_Token_Is_Cancelled_During_Execution()
    {
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var handler = new FakeHandler(async ct =>
        {
            await cts.CancelAsync();
            ct.ThrowIfCancellationRequested();
        });

        var service = CreateService(handler: handler, logger: logger);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("canceled"));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Invoke_Handler_When_Already_Cancelled()
    {
        var handler = new FakeHandler();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = CreateService(handler: handler);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Equal(0, handler.CallCount);
    }

    // ────────────────────────────────────────────────────────── Escalation ──

    [Fact]
    public async Task ExecuteAsync_Should_Notify_Escalation_After_Max_Consecutive_Failures()
    {
        var notifier = new FakeIncidentNotifier();
        using var cts = new CancellationTokenSource();

        var callCount = 0;
        // Fail MaxConsecutiveFailures (3) times, then succeed and cancel
        var handler = new FakeHandler(
            _ => throw new Exception("fail 1"),
            _ => throw new Exception("fail 2"),
            _ => throw new Exception("fail 3"),
            async ct =>
            {
                callCount++;
                await cts.CancelAsync();
            });

        var opts = new WorkerResilienceOptions
        {
            MaxConsecutiveFailures = 3,
            EscalationWindowMinutes = 60,
            EscalationCooldownMinutes = 1
        };

        var service = CreateService(handler: handler, options: opts, notifier: notifier);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.NotEmpty(notifier.Notifications);
        Assert.Equal(3, notifier.Notifications[0].ConsecutiveFailures);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Error_When_Escalation_Notification_Fails()
    {
        var notifier = new FakeIncidentNotifier { ShouldThrow = true };
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var callCount = 0;
        var handler = new FakeHandler(
            _ => throw new Exception("fail 1"),
            _ => throw new Exception("fail 2"),
            _ => throw new Exception("fail 3"),
            async ct =>
            {
                callCount++;
                await cts.CancelAsync();
            });

        var opts = new WorkerResilienceOptions
        {
            MaxConsecutiveFailures = 3,
            EscalationWindowMinutes = 60,
            EscalationCooldownMinutes = 1
        };

        var service = CreateService(handler: handler, options: opts, logger: logger, notifier: notifier);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Error &&
            e.Message.Contains("escalation notification"));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_And_Stop_When_StopServiceOnEscalation_Is_True()
    {
        var notifier = new FakeIncidentNotifier();
        using var cts = new CancellationTokenSource();

        var handler = new FakeHandler(
            _ => throw new Exception("fail 1"),
            _ => throw new Exception("fail 2"),
            _ => throw new Exception("fail 3"));

        var opts = new WorkerResilienceOptions
        {
            MaxConsecutiveFailures = 3,
            EscalationWindowMinutes = 60,
            EscalationCooldownMinutes = 1,
            StopServiceOnEscalation = true
        };

        var service = CreateService(handler: handler, options: opts, notifier: notifier);
        var ex = await RunUntilCancelledAsync(service, cts.Token);

        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Critical_When_StopServiceOnEscalation_Is_True()
    {
        var logger = new FakeLogger();
        using var cts = new CancellationTokenSource();

        var handler = new FakeHandler(
            _ => throw new Exception("fail 1"),
            _ => throw new Exception("fail 2"),
            _ => throw new Exception("fail 3"));

        var opts = new WorkerResilienceOptions
        {
            MaxConsecutiveFailures = 3,
            EscalationWindowMinutes = 60,
            EscalationCooldownMinutes = 1,
            StopServiceOnEscalation = true
        };

        var service = CreateService(handler: handler, options: opts, logger: logger);
        await RunUntilCancelledAsync(service, cts.Token);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Critical &&
            e.Message.Contains("Stopping reconciliation service"));
    }

    // ─────────────────────────────────────────────── Backoff delay formula ──

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(4, 8)]
    [InlineData(5, 16)]
    [InlineData(6, 30)]
    [InlineData(10, 30)]
    public void CalculateFailureDelay_Should_Apply_Exponential_Backoff_Capped_At_30_Minutes(
        int consecutiveFailures, int expectedBaseMinutes)
    {
        // The method is private; we verify it indirectly via observable retry warning log.
        // We use a unit-level reflection test here because it is a pure function with no side effects.
        var method = typeof(ForecastOccurrenceReconciliationService)
            .GetMethod("CalculateFailureDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var delay = (TimeSpan)method.Invoke(null, [consecutiveFailures])!;

        // Jitter adds up to 1 second; the base must equal expectedBaseMinutes minutes.
        var expectedBase = TimeSpan.FromMinutes(expectedBaseMinutes);
        Assert.True(delay >= expectedBase);
        Assert.True(delay <= expectedBase + TimeSpan.FromSeconds(1));
    }
}
