namespace Application.Events.DTOs;

public class TopEventDto
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
}