namespace EventApplication.Exception;

public class EventAlreadyExistsException(string message) : System.Exception(message);