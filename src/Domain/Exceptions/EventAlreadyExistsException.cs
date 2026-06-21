namespace Domain.Exceptions;

public class EventAlreadyExistsException(string message) : Exception(message);