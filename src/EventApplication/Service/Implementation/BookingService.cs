using EventApplication.Exception;
using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

public class BookingService(IEventService eventService) : IBookingService
{
    private ICollection<Booking> Booking { get; } = [];
    
    public async Task<Booking?> CreateBookingAsync(Guid eventId)
    {
        var eventItem = eventService.GetById(eventId);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = eventItem.Id
        };
        
        Booking.Add(booking);
        
        return booking;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = Booking.FirstOrDefault(x => x.Id == bookingId);
        
        if (booking == null)
        {
            throw new BookingNotFoundException($"Не удалось найти бронирование с идентификатором {booking}");
        }
        
        return booking;
    }

    public async Task<ICollection<Booking>> GetPendingBookingsAsync()
    {
        return Booking.Where(x => x.Status == BookingStatus.Pending).ToList();
    }

    public async Task SaveProcessedBookingAsync(Booking processedBooking)
    {
        var bookingToUpdate = Booking.FirstOrDefault(s => s.Id == processedBooking.Id);

        if (bookingToUpdate != null)
        {
            bookingToUpdate.Status = processedBooking.Status;
            bookingToUpdate.ProcessedAt =  processedBooking.ProcessedAt;
        }
    }
}