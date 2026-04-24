namespace MoneyTracker.BusinessLogic.Common.Exceptions;

public sealed class BadRequestException(string message) : Exception(message);
