using Domain.Entities;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services.Implementation;
using Microsoft.EntityFrameworkCore;
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
    
    private readonly User _testUser = new()
    {
        Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
        Login = "test",
        PasswordHash = new PasswordHashGenerator().GenerateHash("test"),
        Role = UserRole.Admin
    };
    
    [Fact]
    public async Task GetPendingBookingsTest()
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await context.Users.AddAsync(_testUser, TestContext.Current.CancellationToken);
        
        await context.Bookings.AddAsync(new Booking
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = _testEvent.Id,
            UserId = _testUser.Id
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
    
    [Fact]
    public async Task AddBookingTest()
    {
        // Arrange
        await using var createContext = await CreateContext();
        await createContext.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await createContext.Users.AddAsync(_testUser, TestContext.Current.CancellationToken);

        var bookingId = Guid.NewGuid();
        await createContext.Bookings.AddAsync(new Booking
        {
            Id = bookingId,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = _testEvent.Id,
            UserId = _testUser.Id
        }, TestContext.Current.CancellationToken);
        
        await createContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        //Act
        await using var verifyContext = await CreateContext();
        var createdBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id ==  bookingId, cancellationToken: TestContext.Current.CancellationToken);
        
        //Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(bookingId, createdBooking.Id);
    }
    
    [Fact]
    public async Task GetByIdTest()
    {
        // Arrange
        await using var createContext = await CreateContext();
        await createContext.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await createContext.Users.AddAsync(_testUser, TestContext.Current.CancellationToken);
        
        var bookingId = Guid.NewGuid();
        await createContext.Bookings.AddAsync(new Booking
        {
            Id = bookingId,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = _testEvent.Id,
            UserId = _testUser.Id
        }, TestContext.Current.CancellationToken);
        
        await createContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        //Act
        await using var verifyContext = await CreateContext();
        var bookingRepository = new BookingRepository(verifyContext);
        var createdBooking = await bookingRepository.GetByIdAsync(bookingId);
        
        //Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(bookingId, createdBooking.Id);
    }
    
    [Fact]
    public async Task SaveChangesTest()
    {
        // Arrange
        await using var createContext = await CreateContext();
        await createContext.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await createContext.Users.AddAsync(_testUser, TestContext.Current.CancellationToken);
        
        var bookingId = Guid.NewGuid();
        await createContext.Bookings.AddAsync(new Booking
        {
            Id = bookingId,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            EventId = _testEvent.Id,
            UserId = _testUser.Id
        }, TestContext.Current.CancellationToken);
        
        await createContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        //Act
        await using var verifyContext = await CreateContext();
        var bookingRepository = new BookingRepository(verifyContext);
        var createdBooking = await bookingRepository.GetByIdAsync(bookingId);
        
        Assert.NotNull(createdBooking);
        createdBooking.Status = BookingStatus.Rejected;
        await bookingRepository.SaveChangesAsync();
        
        var savedBooking = await bookingRepository.GetByIdAsync(bookingId);
        
        //Assert
        Assert.NotNull(savedBooking);
        Assert.Equal(BookingStatus.Rejected, savedBooking.Status);
    }
}