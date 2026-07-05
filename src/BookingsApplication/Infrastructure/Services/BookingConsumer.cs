using System.Text.Json;
using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Interface;
using Common.DomainEvents;
using Common.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class BookingConsumer(IConsumer<Ignore, string> consumer,
    IOptions<KafkaConsumerConfiguration> configuration,
    IServiceProvider serviceProvider, ILogger<BookingConsumer> logger) : IBookingConsumer
{
    public async Task ProcessSuccessfulBookings(CancellationToken cancellationToken)
    {
        var topic =  configuration.Value?.BookingProcessedSuccessfullyTopic;
        consumer.Subscribe(topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var bookingCreatedDomainEvent = JsonSerializer.Deserialize<BookingProcessedDomainEvent>(consumeResult.Message.Value);

                using (var scope = serviceProvider.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var booking = await bookingRepository.GetByIdAsync(bookingCreatedDomainEvent.BookingId);
                    booking.Confirm();
                    booking.ProcessedAt = bookingCreatedDomainEvent.ProcessedAt;
                    await bookingRepository.SaveChangesAsync();
                    
                    consumer.Commit(consumeResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing topic: {topic}");
            }
        }
    }

    public async Task ProcessUnsuccessfulBookings(CancellationToken cancellationToken)
    {
        var topic = configuration.Value?.BookingProcessedUnsuccessfullyTopic;
        consumer.Subscribe(topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var bookingCreatedDomainEvent = JsonSerializer.Deserialize<BookingProcessedDomainEvent>(consumeResult.Message.Value);

                using (var scope = serviceProvider.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var booking = await bookingRepository.GetByIdAsync(bookingCreatedDomainEvent.BookingId);
                    booking.Reject();
                    booking.ProcessedAt = bookingCreatedDomainEvent.ProcessedAt;
                    await bookingRepository.SaveChangesAsync();
                    
                    consumer.Commit(consumeResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing topic: {topic}");
            }
        }
    }

    public void Dispose()
    {
        consumer.Dispose();
    }
}