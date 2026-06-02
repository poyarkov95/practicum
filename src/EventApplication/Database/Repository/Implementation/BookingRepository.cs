using EventApplication.Database.Repository.Interface;
using EventApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApplication.Database.Repository.Implementation;

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
}