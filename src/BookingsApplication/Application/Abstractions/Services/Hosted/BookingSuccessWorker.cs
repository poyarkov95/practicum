using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Hosted;

public class BookingSuccessWorker(ILogger<BookingSuccessWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingSuccessWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var eventConsumer = scope.ServiceProvider.GetRequiredService<IBookingConsumer>();
                await eventConsumer.ProcessSuccessfulBookings(stoppingToken);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при работе BookingSuccessWorker");
            }
        }

        logger.LogInformation("BookingSuccessWorker остановлен");
    }
}