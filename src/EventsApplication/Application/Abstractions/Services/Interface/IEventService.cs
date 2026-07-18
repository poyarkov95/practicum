using Application.Common.DTOs;
using Application.Events.DTOs;
using Common.DomainEvents;

namespace Application.Abstractions.Services.Interface;

public interface IEventService
{
    /// <summary>
    /// Получить список событий
    /// </summary>
    /// <returns>Список событий</returns>
    Task<PaginatedResult<EventInfoDto>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10);
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Возвращает событие</returns>
    Task<EventInfoDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Получить событие из хранилища событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Возвращает событие</returns>
    Task<Domain.Entities.Event> GetEntityByIdAsync(Guid id);
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="model"></param>
    /// <returns>овое событие</returns>
    Task<EventInfoDto> CreateAsync(Domain.Entities.Event model);
    
    /// <summary>
    /// Обновить событе
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Обновленное событие</returns>
    Task<EventInfoDto> UpdateAsync(Domain.Entities.Event model);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Обработать доменное событие создания брони 
    /// </summary>
    Task ProcessCreatedBooking(BookingCreatedDomainEvent booking, CancellationToken token);
    
    /// <summary>
    /// Обработать доменное событие отмены брони
    /// </summary>
    Task ProcessCancelledBooking(BookingCancelledDomainEvent booking, CancellationToken token);
}