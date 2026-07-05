namespace Common.DomainEvents;

public class BookingCreatedDomainEvent
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
}