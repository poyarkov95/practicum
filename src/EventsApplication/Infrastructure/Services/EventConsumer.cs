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
        await Task.Yield();
        
        var topic = configuration.Value?.BookingCreatedTopic;
        
        consumer.Subscribe(topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var bookingCreatedDomainEvent =
                    JsonSerializer.Deserialize<BookingCreatedDomainEvent>(consumeResult.Message.Value);

                if (bookingCreatedDomainEvent == null)
                {
                    return;
                }

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
                logger.LogError(ex, $"Error processing topic : {topic}.  {ex.Message}");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    public async Task ProcessCancelledBookings(CancellationToken cancellationToken)
    {
        var topic = configuration.Value?.BookingCancelledTopic;
        
        consumer.Subscribe(topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var bookingCancelledDomainEvent =
                    JsonSerializer.Deserialize<BookingCancelledDomainEvent>(consumeResult.Message.Value);

                using (var scope = serviceProvider.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    await eventService.ProcessCancelledBooking(bookingCancelledDomainEvent, cancellationToken);

                    // Коммитим оффсет после успешной обработки
                    consumer.Commit(consumeResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing topic : {topic}.  {ex.Message}");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        consumer.Dispose();
    }
}