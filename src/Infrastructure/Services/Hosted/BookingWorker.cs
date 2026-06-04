using Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Hosted;

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

                var tasks =
                    pendingBookings.Select(booking => bookingService.ProcessBookingAsync(booking, stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке бронирования");
            }
        }

        logger.LogInformation("BookingWorker остановлен");
    }
}