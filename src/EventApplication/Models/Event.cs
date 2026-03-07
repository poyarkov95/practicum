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
}