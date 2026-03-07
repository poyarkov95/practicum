using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

/// <summary>
/// Руализация сервиса для работы с событиями
/// </summary>
public class EventService : IEventService
{
    private ICollection<Event> Events { get; } = [];

    public IEnumerable<Event> GetAll()
    {
        return Events;
    }

    public Event? GetById(Guid id)
    {
        var eventItem = Events.FirstOrDefault(x => x.Id == id);
        return eventItem;
    }

    public Event? Create(Event model)
    {
        if (Events.Any(x => x.Id == model.Id))
        {
            return null;
        }
        
        Events.Add(model);
        return model;
    }

    public Event? Update(Event model)
    {
        var eventItem = Events.FirstOrDefault(x => x.Id == model.Id);
        if (eventItem == null) return eventItem;
        
        eventItem.Title = model.Title;
        eventItem.Description = model.Description;
        eventItem.StartAt = model.StartAt;
        eventItem.EndAt = model.EndAt;

        return eventItem;
    }

    public bool Delete(Guid id)
    {
        var eventToDelete = Events.FirstOrDefault(x => x.Id == id);
        return eventToDelete != null && Events.Remove(eventToDelete);
    }
}