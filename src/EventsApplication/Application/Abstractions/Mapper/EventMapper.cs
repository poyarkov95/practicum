using Application.Events.DTOs;
using Domain.Entities;

namespace Application.Abstractions.Mapper;

/// <summary>
/// Маппер моделей
/// </summary>
public static class EventMapper
{
    public static EventInfoDto MapToDto(Event eventItem)
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
    
    public static Event MapToEvent(CreateEventDto eventItem)
    {
        return new Event
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
    
    public static TopEventDto MapToTop10Event(Event eventItem)
    {
        return new TopEventDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
        };
    }
}