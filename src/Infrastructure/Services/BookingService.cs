using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class BookingService(IBookingRepository bookingRepository, ILogger<BookingService> logger, IEventService eventService) : IBookingService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly SemaphoreSlim _createBookingSemaphore = new(1, 1);
    
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
            var eventItem = await eventService.GetEntityByIdAsync(eventId);
            try
            {
                await _createBookingSemaphore.WaitAsync();
                
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
        
               await bookingRepository.AddAsync(booking);
                
                return booking;   
            }
            finally
            {
                _createBookingSemaphore.Release();
            }
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        
        if (booking == null)
        {
            throw new BookingNotFoundException($"Не удалось найти бронирование с идентификатором {booking}");
        }
        
        return booking;
    }

    public async Task<ICollection<Booking>> GetPendingBookingsAsync()
    {
        return await bookingRepository.GetPendingBookingsAsync();
    }

    public async Task SaveProcessedBookingAsync(Booking processedBooking)
    {
        var bookingToUpdate = await bookingRepository.GetByIdAsync(processedBooking.Id);
    
        if (bookingToUpdate != null)
        {
            bookingToUpdate.Status = processedBooking.Status;
            bookingToUpdate.ProcessedAt =  processedBooking.ProcessedAt;
        }
        
        await bookingRepository.SaveChangesAsync();
    }

    public async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Обработка бронирования {Id} для события {EventId}",
                booking.Id, booking.EventId);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            await _processingSemaphore.WaitAsync(stoppingToken);
            
            Event eventItem;

            try
            {
                eventItem = await eventService.GetEntityByIdAsync(booking.EventId);
            }
            catch (EventNotFoundException)
            {
                booking.Reject();
                booking.ProcessedAt = DateTime.UtcNow;
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
            await eventService.UpdateAsync(eventItem);

            logger.LogInformation(
                "Бронирование {Id} обработано успешно", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            //todo - тут мы отменяем бронирование ? или мы уже не успеем этого сделать ?
        }
        catch (System.Exception e)
        {
            logger.LogError(
                "Произошла непредвиденная ошибка при обработке бронирования {Id}. Текст ошибки {errorMessage}",
                booking.Id, e.Message);

            booking.Reject();
            booking.ProcessedAt = DateTime.UtcNow;
            await SaveProcessedBookingAsync(booking);

            var eventItem = await eventService.GetEntityByIdAsync(booking.EventId);
            eventItem.ReleaseSeats();
            await eventService.UpdateAsync(eventItem);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}