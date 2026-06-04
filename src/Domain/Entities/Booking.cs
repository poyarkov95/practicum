using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Модель бронирования
/// </summary>
public class Booking
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    [Comment("Уникальный идентификатор")]
    public Guid Id { get; set; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    [Comment("Текущий статус брони")]
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    [Comment("Дата и время создания брони")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    [Comment("Дата и время обработки брони")]
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    [Comment("Идентификатор события, к которому относится бронь")]
    public Guid EventId { get; set; }

    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; }

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