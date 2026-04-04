using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Hosted;

public class BookingWorker(IBookingService bookingService, ILogger<BookingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var booking = await bookingService.GetPendingBooking();

                if (booking == null)
                {
                    logger.LogInformation("Все бронирования обработаны");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                logger.LogInformation("Обработка бронирования {Id} для события {EventId}",
                    booking.Id, booking.EventId);

                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

                booking.ProcessedAt = DateTime.UtcNow;
                booking.Status = BookingStatus.Confirmed;

                await bookingService.SaveProcessedBooking(booking);
                
                logger.LogInformation(
                    "Бронирование {Id} обработано успешно", booking.Id);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке бронирования");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        logger.LogInformation("BookingWorker остановлен");
    }
}