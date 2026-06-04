using Application.Event.DTOs;

namespace Application.Abstractions.Mapper;

/// <summary>
/// Маппер моделей
/// </summary>
public static class EventMapper
{
    public static EventInfoDto MapToDto(Domain.Entities.Event eventItem)
    {
        return new EventInfoDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt,
            TotalSeats = eventItem.TotalSeats,
            AvailableSeats = eventItem.AvailableSeats
        };
    }
    
    public static Domain.Entities.Event MapToEvent(CreateEventDto eventItem)
    {
        return new Domain.Entities.Event
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt,
            TotalSeats = eventItem.TotalSeats ?? 0,
            AvailableSeats = eventItem.TotalSeats ?? 0
        };
    }
}