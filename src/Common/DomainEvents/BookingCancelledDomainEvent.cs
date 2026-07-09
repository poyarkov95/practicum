namespace Common.DomainEvents;

public class BookingCancelledDomainEvent
{
    /// <summary>
    /// Идентификатор события, для которого отменили бронь
    /// </summary>
    public Guid EventId { get; set; }
}