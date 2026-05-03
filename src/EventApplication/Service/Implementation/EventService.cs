using EventApplication.Database;
using EventApplication.Exception;
using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace EventApplication.Service.Implementation;

/// <summary>
/// Руализация сервиса для работы с событиями
/// </summary>
public class EventService(AppDbContext db) : IEventService
{
    public async Task<PaginatedResult<EventInfoDto>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var query = db.Events.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(s => s.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (from != null)
        {
            query = query.Where(s => s.StartAt >= from);
        }
        
        if (to != null)
        {
            query = query.Where(s => s.EndAt <= to);
        }

        var count = await query.CountAsync();
        
        if (page != null && pageSize != null)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return new PaginatedResult<EventInfoDto>
        {
            Data = (await query.ToListAsync()).Select(EventMapper.MapToDto).ToList(),
            Count = count,
            Page = page.Value,
            PageSize = pageSize.Value
        };
    }

    public async Task<EventInfoDto> GetByIdAsync(Guid id)
    {
        var eventItem = await db.Events.FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }

        return EventMapper.MapToDto(eventItem);
    }

    public async Task<Event> GetEntityByIdAsync(Guid id)
    {
        var eventItem = await db.Events.FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        return eventItem;
    }

    public async Task<EventInfoDto> CreateAsync(Event model)
    {
        if (await db.Events.AnyAsync(x => x.Id == model.Id))
        {
            throw new EventAlreadyExistsException("Событие с таким идентификатором уже существует");
        }
        
        await db.Events.AddAsync(model);
        await db.SaveChangesAsync();
        return EventMapper.MapToDto(model);
    }

    public async Task<EventInfoDto> UpdateAsync(Event model)
    {
        var eventItem = await db.Events.FirstOrDefaultAsync(x => x.Id == model.Id);
        if (eventItem == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {model.Id}");
        }
        eventItem.Title = model.Title;
        eventItem.Description = model.Description;
        eventItem.StartAt = model.StartAt;
        eventItem.EndAt = model.EndAt;
        eventItem.TotalSeats = model.TotalSeats;
        eventItem.AvailableSeats = model.AvailableSeats;

        await db.SaveChangesAsync();
        
        return EventMapper.MapToDto(eventItem);
    }

    public async Task DeleteAsync(Guid id)
    {
        var eventToDelete = await db.Events.FirstOrDefaultAsync(x => x.Id == id);

        if (eventToDelete == null)
        {
            throw new EventNotFoundException($"Не удалось найти событие с идентификатором {id}");
        }
        
        db.Events.Remove(eventToDelete);
        await db.SaveChangesAsync();
    }
}