namespace EventApplication.Exception;

public class NoAvailableSeatsException(string message) : System.Exception(message);