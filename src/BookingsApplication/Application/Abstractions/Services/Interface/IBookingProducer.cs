using Domain.Entities;

namespace Application.Abstractions.Services.Interface;

public interface IBookingProducer : IDisposable
{
    /// <summary>
    /// Опубликовать доменное событие создания брони
    /// </summary>
    Task PublishBookingCreated(Booking booking, CancellationToken ct);
}