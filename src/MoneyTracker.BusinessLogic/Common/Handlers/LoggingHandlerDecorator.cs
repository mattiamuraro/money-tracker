using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MoneyTracker.BusinessLogic.Common.Handlers;

/// <summary>
/// Decorator that wraps any IHandler&lt;TRequest, TResult&gt; with structured
/// logging: operation name, duration, and error details.
/// </summary>
public partial class LoggingHandlerDecorator<TRequest, TResult>(
    IHandler<TRequest, TResult> inner,
    ILogger<LoggingHandlerDecorator<TRequest, TResult>> logger)
    : IHandler<TRequest, TResult>
{
    public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var operationName = typeof(TRequest).Name;
        LogHandlerStarted(logger, operationName);
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await inner.Handle(request, cancellationToken);
            sw.Stop();
            LogHandlerSucceeded(logger, operationName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogHandlerFailed(logger, operationName, sw.ElapsedMilliseconds, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handler started: {OperationName}")]
    private static partial void LogHandlerStarted(ILogger logger, string operationName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handler succeeded: {OperationName} in {ElapsedMs}ms")]
    private static partial void LogHandlerSucceeded(ILogger logger, string operationName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handler failed: {OperationName} after {ElapsedMs}ms")]
    private static partial void LogHandlerFailed(ILogger logger, string operationName, long elapsedMs, Exception exception);
}

/// <summary>
/// Decorator that wraps any IHandler&lt;TRequest&gt; (void) with structured logging.
/// </summary>
public partial class LoggingHandlerDecorator<TRequest>(
    IHandler<TRequest> inner,
    ILogger<LoggingHandlerDecorator<TRequest>> logger)
    : IHandler<TRequest>
{
    public async Task Handle(TRequest request, CancellationToken cancellationToken)
    {
        var operationName = typeof(TRequest).Name;
        LogHandlerStarted(logger, operationName);
        var sw = Stopwatch.StartNew();
        try
        {
            await inner.Handle(request, cancellationToken);
            sw.Stop();
            LogHandlerSucceeded(logger, operationName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogHandlerFailed(logger, operationName, sw.ElapsedMilliseconds, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handler started: {OperationName}")]
    private static partial void LogHandlerStarted(ILogger logger, string operationName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handler succeeded: {OperationName} in {ElapsedMs}ms")]
    private static partial void LogHandlerSucceeded(ILogger logger, string operationName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handler failed: {OperationName} after {ElapsedMs}ms")]
    private static partial void LogHandlerFailed(ILogger logger, string operationName, long elapsedMs, Exception exception);
}