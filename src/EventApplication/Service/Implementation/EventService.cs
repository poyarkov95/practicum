using EventApplication.Database.Repository.Interface;
using EventApplication.Exception;
using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

/// <summary>
/// Руализация сервиса для работы с событиями
/// </summary>
public class EventService(IEventRepository eventRepository) : IEventService
{
    public async Task<PaginatedResult<EventInfoDto>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var events = await eventRepository.GetAllAsync(title, from, to, page, pageSize);
        return new PaginatedResult<EventInfoDto>
        {
            Data = events.Select(EventMapper.MapToDto),
            Count = await eventRepository.CountAsync(),
            Page = page.Value,
            PageSize = pageSize.Value
        };
    }

    public async Task<EventInfoDto> GetByIdAsync(Guid id)
    {
        var eventItem = await eventRepository.GetByIdAsync(id);

        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }

        return EventMapper.MapToDto(eventItem);
    }

    public async Task<Event> GetEntityByIdAsync(Guid id)
    {
        var eventItem = await eventRepository.GetByIdAsync(id);

        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        return eventItem;
    }

    public async Task<EventInfoDto> CreateAsync(Event model)
    {
        if (await eventRepository.GetByIdAsync(model.Id) != null)
        {
            throw new EventAlreadyExistsException("Событие с таким идентификатором уже существует");
        }
        
        var newEvent = await eventRepository.CreateAsync(model);
        return EventMapper.MapToDto(newEvent);
    }

    public async Task<EventInfoDto> UpdateAsync(Event model)
    {
        var eventItem = await eventRepository.GetByIdAsync(model.Id);
        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {model.Id}");
        }
        
        var updatedEvent = await eventRepository.UpdateAsync(model);
        return EventMapper.MapToDto(updatedEvent);
    }

    public async Task DeleteAsync(Guid id)
    {
        var eventToDelete = await eventRepository.GetByIdAsync(id);

        if (eventToDelete == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        await eventRepository.DeleteAsync(eventToDelete);
    }
}