namespace Common.DomainEvents;

public class BookingProcessedDomainEvent : BookingCreatedDomainEvent
{
    public DateTime ProcessedAt { get; set; }
}