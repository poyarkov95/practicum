namespace EventApplication.Models;

/// <summary>
/// Модель бронирования
/// </summary>
public class Booking
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Метод смены статуса на BookingStatus.Confirmed
    /// </summary>
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }
    
    /// <summary>
    /// Метод смены статуса на BookingStatus.Rejected
    /// </summary>
    public void Reject()
    {
        Status = BookingStatus.Rejected;
    }
}