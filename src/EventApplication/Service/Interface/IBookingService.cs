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
     Task<Booking?> GetBookingByIdAsync(Guid bookingId);

     /// <summary>
     /// Получить бронирование в статусе Pending
     /// </summary>
     Task<Booking?> GetPendingBooking();

     /// <summary>
     /// Сохранить в хранилище обработанную бронь
     /// </summary>
     Task SaveProcessedBooking(Booking processedBooking);
}