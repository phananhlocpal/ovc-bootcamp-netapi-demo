namespace Demo.Exceptions;

public sealed class UnauthorizedAppException(string message) : AppException(message, StatusCodes.Status401Unauthorized);
