namespace MoneyTracker.BusinessLogic.Common.Exceptions;

public sealed class ConflictException(string message) : Exception(message);