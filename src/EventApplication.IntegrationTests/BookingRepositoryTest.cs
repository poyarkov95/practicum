using EventApplication.Database.Repository.Implementation;
using EventApplication.Models;
using Xunit;

namespace EventApplication.IntegrationTests;

public class BookingRepositoryTest : DatabaseTestManager
{
    private readonly Event _testEvent = new()
    {
        Id = new Guid("8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
        Title = "Заголовок события",
        Description = "Описание события",
        StartAt = new DateTime(2020, 01, 01).ToUniversalTime(),
        EndAt = new DateTime(2020, 01, 31).ToUniversalTime(),
    };
    
    [Fact]
    public async Task UpdateEventTest()
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await context.Bookings.AddAsync(new Booking
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = _testEvent.Id
        }, TestContext.Current.CancellationToken);
        
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await using var verifyContext = await CreateContext();
        var bookingRepository = new BookingRepository(verifyContext);
        var pendingBookings = await bookingRepository.GetPendingBookingsAsync();
        
        //Assert
        Assert.Single(pendingBookings);
        Assert.Equal(BookingStatus.Pending, pendingBookings.FirstOrDefault()?.Status);
    }
}