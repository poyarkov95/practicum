using EventApplication.Exception;
using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

/// <summary>
/// Руализация сервиса для работы с событиями
/// </summary>
public class EventService : IEventService
{
    private ICollection<Event> Events { get; } = [];

    public PaginatedResult<EventDto> GetAll(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var query = Events as IEnumerable<Event>;

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(s => s.Title == title);
        }

        if (from != null)
        {
            query = query.Where(s => s.StartAt >= from);
        }
        
        if (to != null)
        {
            query = query.Where(s => s.EndAt <= to);
        }

        var count = query.Count();
        
        if (page != null && pageSize != null)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return new PaginatedResult<EventDto>
        {
            Data = query.ToList().Select(EventMapper.MapToDto).ToList(),
            Count = count,
            Page = page.Value,
            PageSize = pageSize.Value
        };
    }

    public EventDto GetById(Guid id)
    {
        var eventItem = Events.FirstOrDefault(x => x.Id == id);

        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }

        return EventMapper.MapToDto(eventItem);
    }

    public EventDto Create(Event model)
    {
        if (Events.Any(x => x.Id == model.Id))
        {
            throw new EventAlreadyExistsException("Событие с таким идентификатором уже существует");
        }
        
        Events.Add(model);
        return EventMapper.MapToDto(model);
    }

    public EventDto Update(Event model)
    {
        var eventItem = Events.FirstOrDefault(x => x.Id == model.Id);
        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {model.Id}");
        }
        eventItem.Title = model.Title;
        eventItem.Description = model.Description;
        eventItem.StartAt = model.StartAt;
        eventItem.EndAt = model.EndAt;

        return EventMapper.MapToDto(eventItem);
    }

    public void Delete(Guid id)
    {
        var eventToDelete = Events.FirstOrDefault(x => x.Id == id);

        if (eventToDelete == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        Events.Remove(eventToDelete);
    }
}