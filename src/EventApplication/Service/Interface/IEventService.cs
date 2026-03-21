using EventApplication.Models;

namespace EventApplication.Service.Interface;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получить список событий
    /// </summary>
    /// <returns>Список событий</returns>
    PaginatedResult<EventDto> GetAll(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10);
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Возвращает событие</returns>
    EventDto GetById(Guid id);
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="model"></param>
    /// <returns>овое событие</returns>
    EventDto Create(Event model);
    
    /// <summary>
    /// Обновить событе
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Обновленное событие</returns>
    EventDto Update(Event model);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    void Delete(Guid id);
}