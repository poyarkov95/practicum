namespace Domain.Exceptions;

public class NoAvailableSeatsException(string message) : System.Exception(message);