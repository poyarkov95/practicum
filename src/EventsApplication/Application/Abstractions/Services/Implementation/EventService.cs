using Application.Abstractions.Mapper;
using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Interface;
using Application.Common.DTOs;
using Application.Events.DTOs;
using Common.DomainEvents;
using Common.Settings;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Abstractions.Services.Implementation;

/// <summary>
/// Руализация сервиса для работы с событиями
/// </summary>
public class EventService(IEventRepository eventRepository, 
    IEventProducer producer, 
    ICacheService cacheService,
    IOptions<CacheSettings> cacheSettings,
    ILogger<IEventService> logger) : IEventService
{
    public async Task<PaginatedResult<EventInfoDto>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var events = await eventRepository.GetAllAsync(title, from, to, page, pageSize);
        return new PaginatedResult<EventInfoDto>
        {
            Data = events.Select(EventMapper.MapToDto),
            Count = await eventRepository.CountAsync(title, from, to, page, pageSize),
            Page = page.Value,
            PageSize = pageSize.Value
        };
    }

    public async Task<EventInfoDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"event:{id}";
        
        var cachedEvent = await cacheService.GetAsync<Event>(cacheKey, cancellationToken);
        if (cachedEvent != null)
        {
            logger.LogDebug("Event {EventId} found in cache", id);
            return EventMapper.MapToDto(cachedEvent);
        }
        
        var eventItem = await eventRepository.GetByIdAsync(id);
        
        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }        
        
        await cacheService.SetAsync(
            cacheKey,
            eventItem,
            TimeSpan.FromSeconds(cacheSettings.Value.EventCacheTTLSeconds),
            cancellationToken);

        return EventMapper.MapToDto(eventItem);
    }

    public async Task<EventInfoDto> CreateAsync(Event model)
    {
        if (await eventRepository.GetByIdAsync(model.Id) != null)
        {
            throw new EventAlreadyExistsException("Событие с таким идентификатором уже существует");
        }
        
        var newEvent = await eventRepository.CreateAsync(model);
        return EventMapper.MapToDto(newEvent);
    }

    public async Task<EventInfoDto> UpdateAsync(Event model)
    {
        var eventItem = await eventRepository.GetByIdAsync(model.Id);
        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {model.Id}");
        }
        
        var updatedEvent = await eventRepository.UpdateAsync(model);
        
        await InvalidateEventCacheAsync(updatedEvent.Id);
        return EventMapper.MapToDto(updatedEvent);
    }

    public async Task DeleteAsync(Guid id)
    {
        var eventToDelete = await eventRepository.GetByIdAsync(id);

        if (eventToDelete == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        await eventRepository.DeleteAsync(eventToDelete);
        
        await InvalidateEventCacheAsync(id);
    }

    public async Task ProcessCreatedBooking(BookingCreatedDomainEvent booking, CancellationToken token)
    {
         var eventItem = await eventRepository.GetByIdAsync(booking.EventId);

         if (eventItem == null)
         {
             await producer.PublishBookingProcessedUnsuccessfully(booking, token);
             logger.LogError($"Не удалось найти событие с идентификатором {booking.EventId}");
             return;
         }
         
         if (eventItem.StartAt <= DateTime.UtcNow)
         {
             await producer.PublishBookingProcessedUnsuccessfully(booking, token);
             logger.LogError("Event already started, booking is unavailable");
             return;
         }
        
         if (!eventItem.TryReserveSeats())
         {
             await producer.PublishBookingProcessedUnsuccessfully(booking, token);
             logger.LogError("No available seats for this event");
             return;
         }
        
         await producer.PublishBookingProcessedSuccessfully(booking, token);
         await eventRepository.UpdateAsync(eventItem);
         
         await InvalidateEventCacheAsync(booking.EventId, token);
    }

    public async Task ProcessCancelledBooking(BookingCancelledDomainEvent booking, CancellationToken token)
    {
        var eventItem = await eventRepository.GetByIdAsync(booking.EventId);

        if (eventItem == null)
        {
            logger.LogError($"Не удалось найти событие с идентификатором {booking.EventId}");
            return;
        }
        
        eventItem.ReleaseSeats();
        
        await eventRepository.UpdateAsync(eventItem);

        await InvalidateEventCacheAsync(booking.EventId, token);
    }

    public async Task<ICollection<TopEventDto>> GetTop10Events(CancellationToken token)
    {
        var cacheKey = "events:top10";
        
        var cachedEvents = await cacheService.GetAsync<List<Event>>(cacheKey, token);
        if (cachedEvents != null)
        {
            logger.LogDebug("Top 10 events found in cache");
            return cachedEvents.Select(EventMapper.MapToTop10Event).ToList();
        }
        var events = await eventRepository.GetTop10Events();

        await cacheService.SetAsync(
            cacheKey,
            events,
            TimeSpan.FromSeconds(cacheSettings.Value.TopEventsCacheTTLSeconds),
            token);

        return events.Select(EventMapper.MapToTop10Event).ToList();
    }

    public async Task InvalidateEventCacheAsync(Guid eventId, CancellationToken token = default)
    {
        var cacheKey = $"event:{eventId}";
        await cacheService.RemoveAsync(cacheKey, token);
        logger.LogDebug("Invalidated cache for event {EventId}", eventId);
    }
}