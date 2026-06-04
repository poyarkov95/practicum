using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Модель события
/// </summary>
public class Event
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    [Comment("Уникальный идентификатор")]
    public required Guid Id { get;set; }
    
    /// <summary>
    /// Заголовок события
    /// </summary>
    [Comment("Заголовок события")]
    public required string Title { get;set; }
    
    /// <summary>
    /// Описание события
    /// </summary>
    [Comment("Описание события")]
    public string? Description  { get;set; }
    
    /// <summary>
    /// Дата начала события 
    /// </summary>
    [Comment("Дата начала события")]
    public required DateTime StartAt { get;set; }
    
    /// <summary>
    /// Дата окончания события 
    /// </summary>
    [Comment("Дата окончания события")]
    public required DateTime EndAt { get;set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    [Comment("Общее количество мест на событии")]
    public int TotalSeats { get; set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    [Comment("Текущее количество свободных мест")]
    public int AvailableSeats { get; set; }

    /// <summary>
    /// Бронирования на событие
    /// </summary>
    public List<Booking> Bookings { get; set; }

    /// <summary>
    /// Метод резерва доступных мест
    /// </summary>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }
        
        AvailableSeats -= count;
        return true;
    }

    /// <summary>
    /// Метод особождения мест
    /// </summary>
    public void ReleaseSeats(int count = 1)
    {
        if (AvailableSeats < TotalSeats)
        {
            AvailableSeats += count;   
        }
    }
}