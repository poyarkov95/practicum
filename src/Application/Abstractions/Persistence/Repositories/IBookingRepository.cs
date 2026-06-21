namespace Application.Abstractions.Persistence.Repositories;

public interface IBookingRepository
{
    /// <summary>
    /// Cоздание брони 
    /// </summary>
    Task<Domain.Entities.Booking> AddAsync(Domain.Entities.Booking booking);
    
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<Domain.Entities.Booking?> GetByIdAsync(Guid bookingId);

    /// <summary>
    /// СОхранить изменения в базе данных
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
    
    /// <summary>
    /// Получить бронирование в статусе Pending
    /// </summary>
    Task<ICollection<Domain.Entities.Booking>> GetPendingBookingsAsync();
    
    Task<ICollection<Domain.Entities.Booking>> GetBookingsAsync();
    
    /// <summary>
    /// Посчитать кол-во броней на одно событие у пользователя
    /// </summary>
    Task<int> CountEventUserBookingsAsync(Guid eventId, Guid userId);
}