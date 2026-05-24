using EventApplication.Database.Repository.Interface;
using EventApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApplication.Database.Repository.Implementation;

public class EventRepository(AppDbContext db) : IEventRepository
{
    public async Task<ICollection<Event>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var query = PrepareQuery(title, from, to);
        
        if (page != null && pageSize != null)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(string? title = null, DateTime? from = null, DateTime? to = null, int? page = 1, int? pageSize = 10)
    {
        var query = PrepareQuery(title, from, to);
        return await query.CountAsync();
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await db.Events.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Event> CreateAsync(Event model)
    {
        var eventItem = await db.Events.AddAsync(model);
        await db.SaveChangesAsync();
        return eventItem.Entity;
    }

    public async Task<Event> UpdateAsync(Event model)
    {
        var eventItem = await db.Events.FirstOrDefaultAsync(x => x.Id == model.Id);
        
        eventItem.Title = model.Title;
        eventItem.Description = model.Description;
        eventItem.StartAt = model.StartAt;
        eventItem.EndAt = model.EndAt;
        eventItem.TotalSeats = model.TotalSeats;
        eventItem.AvailableSeats = model.AvailableSeats;

        await db.SaveChangesAsync();
        
        return eventItem;
    }

    public async Task DeleteAsync(Event eventItem)
    { 
        db.Events.Remove(eventItem);
        await db.SaveChangesAsync();
    }

    private IQueryable<Event> PrepareQuery(string? title = null, DateTime? from = null, DateTime? to = null)
    {
        var query = db.Events.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(s => s.Title.ToLower().Contains(title.ToLower()));
        }

        if (from != null)
        {
            query = query.Where(s => s.StartAt >= from);
        }
        
        if (to != null)
        {
            query = query.Where(s => s.EndAt <= to);
        }
        
        return query;
    }
}