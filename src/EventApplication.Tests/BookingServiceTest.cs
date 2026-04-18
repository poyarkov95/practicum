using EventApplication.Exception;
using EventApplication.Models;
using EventApplication.Service.Hosted;
using EventApplication.Service.Implementation;
using EventApplication.Service.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventApplication.Tests;

public class BookingServiceTest
{
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingService> _logger;
    private readonly Event _testEvent;

    public BookingServiceTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        _eventService = new EventService();
        _logger = new Logger<BookingService>(loggerFactory);
        _bookingService = new BookingService(_logger, _eventService);
        
        _testEvent = new Event
        {
            Id = new Guid("8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31)
        };
        
        _eventService.Create(_testEvent);
    }

    [Fact]
    public async Task CreateBookingTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id);
        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }
    
    [Fact]
    public async Task CreateMultipleBookingTest()
    {
        var bookingFirst = await _bookingService.CreateBookingAsync(_testEvent.Id);
        var bookingSecond = await _bookingService.CreateBookingAsync(_testEvent.Id);
        
        Assert.NotNull(bookingFirst);
        Assert.NotNull(bookingSecond);
        Assert.NotEqual(bookingFirst.Id, bookingSecond.Id);
    }

    [Fact]
    public async Task GetBookingByIdTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id);
        
        Assert.NotNull(booking);
        
        var bookingFromStorage = await _bookingService.GetBookingByIdAsync(booking.Id);
        Assert.Equal(booking.Id, bookingFromStorage?.Id);
    }

    [Fact]
    public async Task VerifyBookingStatusChangeTest()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var createDate = new DateTime(2025, 1, 1);
        var processedDate = createDate.AddSeconds(5);
        
        var mockBookingService = new Mock<IBookingService>();
        mockBookingService
            .Setup(service => service.GetPendingBookingsAsync())
            .ReturnsAsync(new List<Booking>{new() { Id = bookingId, EventId = eventId }});
        
        mockBookingService
            .Setup(service => service.CreateBookingAsync(eventId))
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                CreatedAt = createDate,
                Status = BookingStatus.Pending
            });
        
        mockBookingService
            .Setup(service => service.GetBookingByIdAsync(bookingId))
            .ReturnsAsync(new Booking{
                Id = bookingId, 
                EventId = eventId,
                CreatedAt = createDate,
                Status = BookingStatus.Confirmed,
                ProcessedAt = processedDate });
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(provider => provider.GetService(typeof(IBookingService)))
            .Returns(mockBookingService.Object);
        
        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope
            .Setup(scope => scope.ServiceProvider)
            .Returns(mockServiceProvider.Object);
        
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory
            .Setup(factory => factory.CreateScope())
            .Returns(mockServiceScope.Object);
        
        var mockLogger = new Mock<ILogger<BookingWorker>>();
        
        await mockBookingService.Object.CreateBookingAsync(eventId);
        
        var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);
        
        using var cts = new CancellationTokenSource();
        
        await worker.StartAsync(cts.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
        
        await worker.StopAsync(cts.Token);
        
        var processedBooking = await mockBookingService.Object.GetBookingByIdAsync(bookingId);
        
        Assert.NotNull(processedBooking);
        
        Assert.Equal(BookingStatus.Confirmed, processedBooking.Status);
        Assert.NotNull(processedBooking.ProcessedAt);
    }
    
    [Fact]
    public async Task CreateNonExistingEventBookingTest()
    {
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(Guid.NewGuid()));
    }
    
    [Fact]
    public async Task CreateDeletedEventBookingTest()
    {
        var eventId = Guid.NewGuid();
        var eventItem = new Event
        {
            Id = eventId,
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31)
        };
        
        _eventService.Create(eventItem);
        _eventService.Delete(eventId);
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task SearchByNonExistingBookingIdTest()
    {
        await Assert.ThrowsAsync<BookingNotFoundException>(() => _bookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }
}