namespace Application.Events.DTOs;

/// <summary>
/// DTO Модель события
/// </summary>
public class EventInfoDto
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public Guid Id { get;set; }
    
    /// <summary>
    /// Заголовок события
    /// </summary>
    public required string Title { get;set; }
    
    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description  { get;set; }
    
    /// <summary>
    /// Дата начала события 
    /// </summary>
    public required DateTime StartAt { get;set; }
    
    /// <summary>
    /// Дата окончания события 
    /// </summary>
    public required DateTime EndAt { get;set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; set; }
    
    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; set; }
}