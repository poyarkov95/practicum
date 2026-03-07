using System.ComponentModel.DataAnnotations;

namespace EventApplication.Models;

/// <summary>
/// DTO Модель события
/// </summary>
public class EventDto : IValidatableObject
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public Guid Id { get;set; } = Guid.NewGuid();
    
    /// <summary>
    /// Заголовок события
    /// </summary>
    [Required(AllowEmptyStrings = true, ErrorMessage = "Заголовок обязателен для заполнения")]
    public required string Title { get;set; }
    
    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description  { get;set; }
    
    /// <summary>
    /// Дата начала события 
    /// </summary>
    [Required(ErrorMessage = "Дата начала события обязательна для заполнения")]
    public required DateTime StartAt { get;set; }
    
    /// <summary>
    /// Дата окончания события 
    /// </summary>
    [Required(ErrorMessage = "Дата окончания события обязательна для заполнения")]
    public required DateTime EndAt { get;set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt должно быть позже StartAt.",
                new[] { nameof(EndAt) }
            );
        }
    }
}