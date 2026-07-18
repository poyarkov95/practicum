namespace Application.Abstractions.Services.Interface;

public interface IBookingConsumer : IDisposable
{
    /// <summary>
    /// Чтение успешных обработанных бронирований из топика
    /// </summary>
    Task ProcessSuccessfulBookings(CancellationToken cancellationToken);
    
    /// <summary>
    /// Чтение неуспешных обработанных бронирований из топика
    /// </summary>
    Task ProcessUnsuccessfulBookings(CancellationToken cancellationToken);
}