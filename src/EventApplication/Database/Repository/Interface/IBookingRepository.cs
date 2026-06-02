using EventApplication.Models;

namespace EventApplication.Database.Repository.Interface;

public interface IBookingRepository
{
    /// <summary>
    /// Cоздание брони 
    /// </summary>
    Task<Booking> AddAsync(Booking booking);
    
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<Booking?> GetByIdAsync(Guid bookingId);

    /// <summary>
    /// СОхранить изменения в базе данных
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
    
    /// <summary>
    /// Получить бронирование в статусе Pending
    /// </summary>
    Task<ICollection<Booking>> GetPendingBookingsAsync();
}