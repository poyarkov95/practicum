namespace Domain.Exceptions;

public class EventAlreadyExistsException(string message) : System.Exception(message);