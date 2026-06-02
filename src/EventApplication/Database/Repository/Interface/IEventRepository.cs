using EventApplication.Models;

namespace EventApplication.Database.Repository.Interface;

public interface IEventRepository
{
    /// <summary>
    /// Получить список событий
    /// </summary>
    /// <returns>Список событий</returns>
    Task<ICollection<Event>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10);
    
    /// <summary>
    /// Подсчитать кол-во событий для пагинации
    /// </summary>
    /// <returns></returns>
    Task<int> CountAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10);
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Возвращает событие</returns>
    Task<Event?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="model"></param>
    /// <returns>овое событие</returns>
    Task<Event> CreateAsync(Event model);
    
    /// <summary>
    /// Обновить событе
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Обновленное событие</returns>
    Task<Event> UpdateAsync(Event model);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    Task DeleteAsync(Event eventItem);
}