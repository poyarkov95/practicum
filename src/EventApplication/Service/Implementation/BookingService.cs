using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

public class BookingService(IEventService eventService) : IBookingService
{
    private ICollection<Booking> Booking { get; } = [];
    
    public async Task<Booking?> CreateBookingAsync(Guid eventId)
    {
        var eventItem = eventService.GetById(eventId);
        if (eventItem == null)
        {
            return null;
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = eventId
        };
        
        Booking.Add(booking);
        
        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = Booking.FirstOrDefault(x => x.Id == bookingId);
        return booking;
    }

    public async Task<Booking?> GetPendingBooking()
    {
        return Booking.FirstOrDefault(x => x.Status == BookingStatus.Pending);
    }

    public async Task SaveProcessedBooking(Booking processedBooking)
    {
        var bookingToUpdate = Booking.FirstOrDefault(s => s.Id == processedBooking.Id);

        if (bookingToUpdate != null)
        {
            bookingToUpdate.Status = processedBooking.Status;
            bookingToUpdate.ProcessedAt =  processedBooking.ProcessedAt;
        }
    }
}