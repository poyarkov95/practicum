namespace Domain.Exceptions;

public class BookingNotFoundException(string message) : System.Exception(message);