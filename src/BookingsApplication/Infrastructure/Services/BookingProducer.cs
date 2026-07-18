using System.Text.Json;
using Application.Abstractions.Services.Interface;
using Common.DomainEvents;
using Common.Settings;
using Confluent.Kafka;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class BookingProducer(ILogger<BookingProducer> logger, IOptions<KafkaConfiguration> configuration, IProducer<Null, string> producer) : IBookingProducer
{
    public async Task PublishBookingCreated(Booking booking, CancellationToken ct)
    {
        var message = new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(new BookingCreatedDomainEvent
            {
                BookingId = booking.Id,
                EventId = booking.EventId
            })
        };
        
        try
        {
            await producer.ProduceAsync(configuration.Value?.BookingCreatedTopic, message, ct);
        }
        catch (ProduceException<Null, string> ex)
        {
            logger.LogError(ex, "Failed to publish BookingCreated event");
            throw;
        }
    }

    public async Task PublishBookingCancelled(Booking booking, CancellationToken ct)
    {
        var message = new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(new BookingCancelledDomainEvent
            {
                EventId = booking.EventId
            })
        };
        
        try
        {
            await producer.ProduceAsync(configuration.Value?.BookingCancelledTopic, message, ct);
        }
        catch (ProduceException<Null, string> ex)
        {
            logger.LogError(ex, "Failed to publish BookingCancelled event");
            throw;
        }
    }

    public void Dispose()
    {
        producer.Dispose();
    }
}