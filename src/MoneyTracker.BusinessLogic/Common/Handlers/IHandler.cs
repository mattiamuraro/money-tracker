namespace MoneyTracker.BusinessLogic.Common.Handlers;

/// <summary>
/// Marker interface for a handler that returns a result.
/// </summary>
public interface IHandler<TRequest, TResult>
{
    Task<TResult> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Marker interface for a handler that returns no result (void/Task).
/// </summary>
public interface IHandler<TRequest>
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}