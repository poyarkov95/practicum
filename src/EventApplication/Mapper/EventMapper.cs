using EventApplication.Models;

namespace EventApplication.Mapper;

/// <summary>
/// Маппер моделей
/// </summary>
public static class EventMapper
{
    public static EventDto MapToDto(Event eventItem)
    {
        return new EventDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt
        };
    }

    public static Event MapToEvent(EventDto eventDto)
    {
        return new Event
        {
            Id = eventDto.Id,
            Title = eventDto.Title,
            Description = eventDto.Description,
            StartAt = eventDto.StartAt,
            EndAt = eventDto.EndAt
        };
    }
}