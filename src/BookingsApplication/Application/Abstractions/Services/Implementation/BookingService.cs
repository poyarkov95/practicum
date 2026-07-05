using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Interface;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Implementation;

public class BookingService(IBookingRepository bookingRepository, ILogger<BookingService> logger, IBookingProducer producer) : IBookingService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly SemaphoreSlim _createBookingSemaphore = new(1, 1);
    private const int BookingPerEventLimit = 10;
    
    public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId)
    {
            try
            {
                await _createBookingSemaphore.WaitAsync();
                
                var eventUserBookings = await bookingRepository.CountEventUserBookingsAsync(userId);
                
                 if (eventUserBookings == BookingPerEventLimit)
                 {
                     throw new BookingLimitExceededException($"Booking limit exceeded. Only {BookingPerEventLimit} bookings available for each user per event");
                 }
               
                 var booking = new Booking
                 {
                     Id = Guid.NewGuid(),
                     CreatedAt = DateTime.UtcNow,
                     Status = BookingStatus.Pending,
                     EventId = eventId,
                     UserId = userId
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
            throw new BookingNotFoundException($"Не удалось найти бронирование с идентификатором {bookingId}");
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
            
            //помечаем, чтобы воркер не захватывал это бронирование
            booking.Confirm();
            await SaveProcessedBookingAsync(booking);
            
            await producer.PublishBookingCreated(booking, stoppingToken);
            
            //Domain.Entities.Event eventItem;

            // try
            // {
            //     eventItem = await eventService.GetEntityByIdAsync(booking.EventId);
            // }
            // catch (EventNotFoundException)
            // {
            //     booking.Reject();
            //     booking.ProcessedAt = DateTime.UtcNow;
            //     await SaveProcessedBookingAsync(booking);
            //
            //     logger.LogWarning(
            //         "Обработка бронирования {Id} для события {EventId} прошла неудачно. Событие не найдено",
            //         booking.Id, booking.EventId);
            //
            //     return;
            // 
            //await eventService.UpdateAsync(eventItem);

            logger.LogInformation(
                "Бронирование {Id} обработано успешно", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            //todo - тут мы отменяем бронирование ? или мы уже не успеем этого сделать ?
        }
        catch (Exception e)
        {
            logger.LogError(
                "Произошла непредвиденная ошибка при обработке бронирования {Id}. Текст ошибки {errorMessage}",
                booking.Id, e.Message);

            booking.Reject();
            booking.ProcessedAt = DateTime.UtcNow;
            await SaveProcessedBookingAsync(booking);

            // var eventItem = await eventService.GetEntityByIdAsync(booking.EventId);
            // eventItem.ReleaseSeats();
            // await eventService.UpdateAsync(eventItem);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid userId)
    {
        // var currentUser = await userService.GetUser(userId);
        // var booking = await bookingRepository.GetByIdAsync(bookingId);
        //
        // if (booking == null)
        // {
        //     throw new BookingNotFoundException($"Не удалось найти бронирование с идентификатором {bookingId}");
        // }
        //
        // if (booking.Status == BookingStatus.Cancelled)
        // {
        //     return;
        // }
        //
        // ValidateBookingCancel(booking, currentUser);
        //
        // booking.Cancel();
        //
        // var eventItem = await eventService.GetEntityByIdAsync(booking.EventId);
        // eventItem.ReleaseSeats();
        // await eventService.UpdateAsync(eventItem);
        //
        // await bookingRepository.SaveChangesAsync();
    }

    public void ValidateBookingCancel(Booking booking
        // , Domain.Entities.User currentUser
        )
    {
        // if (currentUser.Role == UserRole.Admin)
        // {
        //     return;
        // }
        //
        // if (booking.UserId != currentUser.Id)
        // {
        //     throw new OperationNotAllowedException("This booking belongs to another user and cannot be canceled");
        // }
    }
}