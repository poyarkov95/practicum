using System.Text.Json;
using Application.Abstractions.Services.Interface;
using Common.DomainEvents;
using Common.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EventProducer(ILogger<IEventProducer> logger,
    IOptions<KafkaConfiguration> configuration,
    IProducer<Null, string> producer) : IEventProducer
{
    public async Task PublishBookingProcessedSuccessfully(BookingCreatedDomainEvent domainEvent,  CancellationToken token)
    {
        await PublishMessage(configuration.Value.BookingProcessedSuccessfullyTopic, domainEvent, token);
    }

    public async Task PublishBookingProcessedUnsuccessfully(BookingCreatedDomainEvent domainEvent,  CancellationToken token)
    {
       await PublishMessage(configuration.Value.BookingProcessedUnsuccessfullyTopic, domainEvent, token);
    }

    private async Task PublishMessage(string topic, BookingCreatedDomainEvent domainEvent,  CancellationToken token)
    {
        var message = new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(new BookingProcessedDomainEvent
            {
                BookingId = domainEvent.BookingId,
                EventId = domainEvent.EventId,
                ProcessedAt = DateTime.UtcNow
            })
        };
        
        try
        {
            await producer.ProduceAsync(topic, message, token);
        }
        catch (ProduceException<Null, string> ex)
        {
            logger.LogError(ex, $"Failed to publish {topic} event");
        }
    }
}