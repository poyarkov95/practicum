using System.Text.Json;
using Application.Abstractions.Services.Interface;
using Common.DomainEvents;
using Common.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EventConsumer(IConsumer<Ignore, string> consumer,
    IOptions<KafkaConsumerConfiguration> configuration,
    IServiceProvider serviceProvider, ILogger<EventConsumer> logger) : IEventConsumer
{
    public async Task ProcessBookings(CancellationToken cancellationToken)
    {
        consumer.Subscribe(configuration.Value?.BookingCreatedTopic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var bookingCreatedDomainEvent = JsonSerializer.Deserialize<BookingCreatedDomainEvent>(consumeResult.Message.Value);

                using (var scope = serviceProvider.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    await eventService.ProcessCreatedBooking(bookingCreatedDomainEvent, cancellationToken);

                    // Коммитим оффсет после успешной обработки
                    consumer.Commit(consumeResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");
            }
        }
    }
    
    public void Dispose()
    {
        consumer.Dispose();
    }
}