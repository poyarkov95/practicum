using EventApplication.Models;

namespace EventApplication.Service.Interface;

public interface IBookingService
{
     /// <summary>
     ///  Cоздание брони для указанного события
     /// </summary>
     Task<Booking?> CreateBookingAsync(Guid eventId);

     /// <summary>
     /// Получение брони по идентификатору
     /// </summary>
     Task<Booking> GetBookingByIdAsync(Guid bookingId);

     /// <summary>
     /// Получить бронирование в статусе Pending
     /// </summary>
     Task<ICollection<Booking>> GetPendingBookingsAsync();

     /// <summary>
     /// Сохранить в хранилище обработанную бронь
     /// </summary>
     Task SaveProcessedBookingAsync(Booking processedBooking);

     /// <summary>
     /// Метод обработки брони в hosted service
     /// </summary>
     Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken);
}