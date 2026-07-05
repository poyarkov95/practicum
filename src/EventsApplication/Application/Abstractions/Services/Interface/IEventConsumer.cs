namespace Application.Abstractions.Services.Interface;

public interface IEventConsumer : IDisposable
{
    /// <summary>
    /// Чтение созданых бронирований из топика
    /// </summary>
    Task ProcessBookings(CancellationToken cancellationToken);
}