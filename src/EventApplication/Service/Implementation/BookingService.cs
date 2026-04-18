using EventApplication.Exception;
using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Implementation;

public class BookingService(ILogger<BookingService> logger, IEventService eventService) : IBookingService
{
    private ICollection<Booking> Booking { get; } = [];
    private readonly object _bookingLock = new();
    
    public async Task<Booking?> CreateBookingAsync(Guid eventId)
    {
            var eventItem = eventService.GetEntityById(eventId);

            lock (_bookingLock)
            {
                if (!eventItem.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }
                
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

    public async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Обработка бронирования {Id} для события {EventId}",
                booking.Id, booking.EventId);

            Event eventItem;

            try
            {
                eventItem = eventService.GetEntityById(booking.EventId);
            }
            catch (EventNotFoundException)
            {
                booking.Reject();
                await SaveProcessedBookingAsync(booking);

                logger.LogWarning(
                    "Обработка бронирования {Id} для события {EventId} прошла неудачно. Событие не найдено",
                    booking.Id, booking.EventId);

                return;
            }

            booking.Confirm();
            booking.ProcessedAt = DateTime.UtcNow;
            booking.Status = BookingStatus.Confirmed;
            
            await SaveProcessedBookingAsync(booking);
            eventService.Update(eventItem);
            
            logger.LogInformation(
                "Бронирование {Id} обработано успешно", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            //todo - тут мы отменяем бронирование ? или мы уже не успеем этого сделать ?
        }
        catch (System.Exception e)
        {
            logger.LogError("Произошла непредвиденная ошибка при обработке бронирования {Id}. Текст ошибки {errorMessage}",
                booking.Id, e.Message);
            
            booking.Reject();
            await SaveProcessedBookingAsync(booking);
            
            var eventItem = eventService.GetEntityById(booking.EventId);
            eventItem.ReleaseSeats();
            eventService.Update(eventItem);
        }
    }
}