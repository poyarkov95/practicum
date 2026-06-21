namespace Domain.Exceptions;

public class OperationNotAllowedException(string message) : Exception(message);