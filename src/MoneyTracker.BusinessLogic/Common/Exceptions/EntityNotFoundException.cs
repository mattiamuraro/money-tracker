namespace MoneyTracker.BusinessLogic.Common.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(
        string message,
        string? errorCode = null,
        string? entityName = null,
        string? entityId = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        EntityName = entityName;
        EntityId = entityId;
    }

    public string? ErrorCode { get; }

    public string? EntityName { get; }

    public string? EntityId { get; }
}
