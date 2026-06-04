using Application.Booking.DTOs;

namespace Application.Abstractions.Mapper;

/// <summary>
/// Маппер моделей 
/// </summary>
public class BookingMapper
{
    public static BookingInfo MapToDto(Domain.Entities.Booking booking)
    {
        return new BookingInfo
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt =  booking.ProcessedAt
        };
    }
}