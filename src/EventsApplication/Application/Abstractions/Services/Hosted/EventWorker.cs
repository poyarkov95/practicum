using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Hosted;

public class EventWorker(ILogger<EventWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventWorker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                await eventConsumer.ProcessBookings(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при работе EventConsumer");
            }
        }

        logger.LogInformation("EventWorker остановлен");
    }
}