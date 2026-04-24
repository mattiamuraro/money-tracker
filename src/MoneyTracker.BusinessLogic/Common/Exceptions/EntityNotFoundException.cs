namespace MoneyTracker.BusinessLogic.Common.Exceptions;

public sealed class EntityNotFoundException(string message) : Exception(message);
