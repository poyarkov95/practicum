using Domain.Entities;

namespace Application.Abstractions.Services.Interface;

public interface IBookingProducer : IDisposable
{
    /// <summary>
    /// Опубликовать доменное событие создания брони
    /// </summary>
    Task PublishBookingCreated(Booking booking, CancellationToken ct);
    
    /// <summary>
    /// Опубликовать доменное событие отмены брони
    /// </summary>
    Task PublishBookingCancelled(Booking booking, CancellationToken ct);
}