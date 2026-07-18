using Common.DomainEvents;

namespace Application.Abstractions.Services.Interface;

public interface IEventProducer
{
    /// <summary>
    /// Опубликовать событие об успешной обработке брони
    /// </summary>
    Task PublishBookingProcessedSuccessfully(BookingCreatedDomainEvent domainEvent, CancellationToken token);
    
    /// <summary>
    /// Опубликовать событие об неуспешной обработке брони
    /// </summary>
    Task PublishBookingProcessedUnsuccessfully(BookingCreatedDomainEvent domainEvent,  CancellationToken token);
}