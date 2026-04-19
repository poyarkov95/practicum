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
    private readonly Event _testEvent;

    public BookingServiceTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = new Logger<BookingService>(loggerFactory);

        _eventService = new EventService();
        _bookingService = new BookingService(logger, _eventService);

        _testEvent = new Event
        {
            Id = new Guid("8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
            TotalSeats = 2,
            AvailableSeats = 2
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
        var availableSeatsBeforeBooking = _testEvent.AvailableSeats;

        var eventService = new Mock<IEventService>();
        eventService.Setup(r => r.GetEntityById(_testEvent.Id)).Returns(_testEvent);

        var bookingLogger = new Logger<BookingService>(new LoggerFactory());

        var realBookingService = new BookingService(bookingLogger, eventService.Object);
        var pendingBooking =  await realBookingService.CreateBookingAsync(_testEvent.Id);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IBookingService)))
            .Returns(realBookingService);

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        var mockLogger = new Mock<ILogger<BookingWorker>>();
        var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);

        using var cts = new CancellationTokenSource();

        await worker.StartAsync(cts.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
        cts.Cancel();

        Assert.NotNull(pendingBooking);
        Assert.Equal(BookingStatus.Confirmed, pendingBooking.Status);
        Assert.NotNull(pendingBooking.ProcessedAt);
        Assert.Equal(_testEvent.AvailableSeats, availableSeatsBeforeBooking - 1);
    }

    [Fact]
    public async Task VerifyEventNotFoundTriggersBookingRejectTest()
    {
        var eventService = new Mock<IEventService>();
        eventService.Setup(r => r.GetEntityById(_testEvent.Id)).Returns(_testEvent);

        var bookingLogger = new Logger<BookingService>(new LoggerFactory());

        var realBookingService = new BookingService(bookingLogger, eventService.Object);
        var pendingBooking =  await realBookingService.CreateBookingAsync(_testEvent.Id);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IBookingService)))
            .Returns(realBookingService); 

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        eventService.Setup(r => r.GetEntityById(_testEvent.Id)).Throws(new EventNotFoundException($"Не удалось найти событие с идентификатором {_testEvent.Id}"));
        
        var mockLogger = new Mock<ILogger<BookingWorker>>();
        var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);

        using var cts = new CancellationTokenSource();

        await worker.StartAsync(cts.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
        cts.Cancel();

        Assert.NotNull(pendingBooking);
        Assert.Equal(BookingStatus.Rejected, pendingBooking.Status);
        Assert.NotNull(pendingBooking.ProcessedAt);
    }
    
    [Fact]
    public async Task VerifyBookingRejectTriggersReleaseSeatsTest()
    {
        var availableSeats = _testEvent.AvailableSeats;
        
        var eventService = new Mock<IEventService>();
        eventService.Setup(r => r.GetEntityById(_testEvent.Id)).Returns(_testEvent);

        var bookingLogger = new Logger<BookingService>(new LoggerFactory());

        var realBookingService = new BookingService(bookingLogger, eventService.Object);
        var pendingBooking =  await realBookingService.CreateBookingAsync(_testEvent.Id);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IBookingService)))
            .Returns(realBookingService); 

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        eventService.Setup(r => r.Update(It.IsAny<Event>())).Throws(new System.Exception());
        
        var mockLogger = new Mock<ILogger<BookingWorker>>();
        var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);

        using var cts = new CancellationTokenSource();

        await worker.StartAsync(cts.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
        cts.Cancel();

        Assert.NotNull(pendingBooking);
        Assert.Equal(BookingStatus.Rejected, pendingBooking.Status);
        Assert.NotNull(pendingBooking.ProcessedAt);
        Assert.Equal(_testEvent.AvailableSeats, availableSeats);
    }
    
    [Fact]
    public async Task VerifyAfterReleaseSeatsBookingIsAvailableTest()
    {
        _testEvent.AvailableSeats = 1;
        var availableSeats = _testEvent.AvailableSeats;
        
        var eventService = new Mock<IEventService>();
        eventService.Setup(r => r.GetEntityById(_testEvent.Id)).Returns(_testEvent);

        var bookingLogger = new Logger<BookingService>(new LoggerFactory());

        var realBookingService = new BookingService(bookingLogger, eventService.Object);
        var pendingBooking =  await realBookingService.CreateBookingAsync(_testEvent.Id);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IBookingService)))
            .Returns(realBookingService); 

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        eventService.Setup(r => r.Update(It.IsAny<Event>())).Throws(new System.Exception());
        
        var mockLogger = new Mock<ILogger<BookingWorker>>();
        var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);

        using var cts = new CancellationTokenSource();

        await worker.StartAsync(cts.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
        cts.Cancel();

        Assert.NotNull(pendingBooking);
        Assert.Equal(BookingStatus.Rejected, pendingBooking.Status);
        Assert.NotNull(pendingBooking.ProcessedAt);
        Assert.Equal(_testEvent.AvailableSeats, availableSeats);
        
        var secondPendingBooking =  await realBookingService.CreateBookingAsync(_testEvent.Id);
        
        eventService.Setup(r => r.Update(It.IsAny<Event>())).Returns(It.IsAny<EventInfoDto>());
        using var ctsNew = new CancellationTokenSource();

        await worker.StartAsync(ctsNew.Token);
        
        await Task.Delay(TimeSpan.FromSeconds(4), ctsNew.Token);
        ctsNew.Cancel();
        
        Assert.NotNull(secondPendingBooking);
        Assert.Equal(BookingStatus.Confirmed, secondPendingBooking.Status);
        Assert.NotNull(secondPendingBooking.ProcessedAt);
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

    [Fact]
    public async Task AvailableSeatsBecomeLessAfterBookingTest()
    {
        var seatsBeforeBooking = _testEvent.AvailableSeats;
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id);

        Assert.NotNull(booking);
        Assert.Equal(_testEvent.AvailableSeats, seatsBeforeBooking - 1);
    }

    [Fact]
    public async Task BookingCreateWithUniqueIds()
    {
        var firstBooking = await _bookingService.CreateBookingAsync(_testEvent.Id);
        var secondBooking = await _bookingService.CreateBookingAsync(_testEvent.Id);

        Assert.NotNull(firstBooking);
        Assert.NotNull(secondBooking);

        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
    }

    [Fact]
    public async Task NotAvailableSeatsThrowTest()
    {
        await _bookingService.CreateBookingAsync(_testEvent.Id);
        await _bookingService.CreateBookingAsync(_testEvent.Id);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(_testEvent.Id));
    }

    [Fact]
    public async Task BookingWithNoAvailableSeats()
    {
        _testEvent.AvailableSeats = 0;
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(_testEvent.Id));
    }

    [Fact]
    public async Task BookingChangeStatusMethodsTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id);

        Assert.NotNull(booking);

        booking.Confirm();
        Assert.Equal(BookingStatus.Confirmed, booking.Status);

        booking.Reject();
        Assert.Equal(BookingStatus.Rejected, booking.Status);
    }
}