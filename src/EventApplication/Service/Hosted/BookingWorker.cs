using EventApplication.Models;
using EventApplication.Service.Interface;

namespace EventApplication.Service.Hosted;

public class BookingWorker(ILogger<BookingWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                
                var pendingBookings = await bookingService.GetPendingBookingsAsync();

                if (pendingBookings.Count == 0)
                {
                    logger.LogInformation("Все бронирования обработаны");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }
                
                foreach (var booking in pendingBookings)
                {
                    logger.LogInformation("Обработка бронирования {Id} для события {EventId}",
                        booking.Id, booking.EventId);

                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

                    booking.ProcessedAt = DateTime.UtcNow;
                    booking.Status = BookingStatus.Confirmed;

                    await bookingService.SaveProcessedBookingAsync(booking);
                
                    logger.LogInformation(
                        "Бронирование {Id} обработано успешно", booking.Id);
                }
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