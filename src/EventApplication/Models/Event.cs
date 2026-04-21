namespace EventApplication.Models;

/// <summary>
/// Модель события
/// </summary>
public class Event
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public required Guid Id { get;set; }
    
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