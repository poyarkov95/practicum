using Application.Abstractions.Persistence.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    public async Task<Booking> AddAsync(Booking booking)
    {
        var createdBooking = await db.Bookings.AddAsync(booking);
        await db.SaveChangesAsync();
        return createdBooking.Entity;
    }

    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task<ICollection<Booking>> GetPendingBookingsAsync()
    {
        return await db.Bookings
            .Where(x => x.Status == BookingStatus.Pending)
            .ToListAsync();
    }

    public async Task<ICollection<Domain.Entities.Booking>> GetBookingsAsync()
    {
        return await db.Bookings.ToListAsync();
    }

    public async Task<int> CountEventUserBookingsAsync(Guid eventId, Guid userId)
    {
        return await db.Bookings
            .Where(x => (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Confirmed)
                        &&  x.UserId == userId).CountAsync();
    }
}