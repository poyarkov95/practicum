namespace Domain.Exceptions;

public class EventNotFoundException(string message) : System.Exception(message);