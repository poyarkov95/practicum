using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Hosted;

public class BookingUnsuccessWorker(ILogger<BookingSuccessWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingUnsuccessWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var eventConsumer = scope.ServiceProvider.GetRequiredService<IBookingConsumer>();
                await eventConsumer.ProcessUnsuccessfulBookings(stoppingToken);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при работе BookingUnsuccessWorker");
            }
        }

        logger.LogInformation("BookingUnsuccessWorker остановлен");
    }
}