using Domain.Entities;

namespace Application.Abstractions.Services.Interface;

public interface IBookingService
{
    /// <summary>
    ///  Cоздание брони для указанного события
    /// </summary>
    Task<Domain.Entities.Booking> CreateBookingAsync(Guid eventId);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<Domain.Entities.Booking> GetBookingByIdAsync(Guid bookingId);

    /// <summary>
    /// Получить бронирование в статусе Pending
    /// </summary>
    Task<ICollection<Domain.Entities.Booking>> GetPendingBookingsAsync();

    /// <summary>
    /// Сохранить в хранилище обработанную бронь
    /// </summary>
    Task SaveProcessedBookingAsync(Domain.Entities.Booking processedBooking);

    /// <summary>
    /// Метод обработки брони в hosted service
    /// </summary>
    Task ProcessBookingAsync(Domain.Entities.Booking booking, CancellationToken stoppingToken);
}