using Domain.Entities;

namespace Application.Abstractions.Persistence.Repositories;

public interface IEventRepository
{
    /// <summary>
    /// Получить список событий
    /// </summary>
    /// <returns>Список событий</returns>
    Task<ICollection<Domain.Entities.Event>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10);
    
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
    Task<Domain.Entities.Event?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="model"></param>
    /// <returns>овое событие</returns>
    Task<Domain.Entities.Event> CreateAsync(Domain.Entities.Event model);
    
    /// <summary>
    /// Обновить событе
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Обновленное событие</returns>
    Task<Domain.Entities.Event> UpdateAsync(Domain.Entities.Event model);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    Task DeleteAsync(Domain.Entities.Event eventItem);
}