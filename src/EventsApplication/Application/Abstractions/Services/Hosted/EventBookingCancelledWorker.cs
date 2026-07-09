using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Hosted;

public class EventBookingCancelledWorker(ILogger<EventBookingCancelledWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventBookingCancelledWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                await eventConsumer.ProcessCancelledBookings(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при работе EventBookingCancelledWorker");
            }
        }

        logger.LogInformation("EventBookingCancelledWorker остановлен");
    }
}