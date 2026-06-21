using System.Collections.Concurrent;
using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Hosted;
using Application.Abstractions.Services.Implementation;
using Application.Abstractions.Services.Interface;
using Application.Event.DTOs;
using Application.User;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventApplication.Tests;

public class BookingServiceTest : IAsyncLifetime
{
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;
    private readonly Event _testEvent;
    private readonly User _adminUser;
    private readonly User _user;
    
    private readonly AppDbContext _dbContext;

    public BookingServiceTest()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase(dbName));
        
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJWTGenerator, JWTGenerator>();
        services.AddScoped<IPasswordHashGenerator, PasswordHashGenerator>();
        services.AddScoped<IUserService, UserService>();

        services.AddLogging(configure => configure.AddConsole());
        
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        _bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        _userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        _testEvent = new Event
        {
            Id = new Guid("8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2030, 01, 01),
            EndAt = new DateTime(2030, 01, 31),
            TotalSeats = 2,
            AvailableSeats = 2
        };

        _adminUser = new User
        {
            Login = "admin",
            Role = UserRole.Admin
        };
        
        _user = new User
        {
            Login = "user",
            Role = UserRole.User
        };
    }
    
    public async ValueTask DisposeAsync()
    {
        //await _dbContext.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _eventService.CreateAsync(_testEvent);
        _adminUser.Id = await _userService.Register(new LoginRequestDto
        {
            Login = _adminUser.Login,
            Password = "test",
            Role = _adminUser.Role
        });
        
        _user.Id = await _userService.Register(new LoginRequestDto
        {
            Login = _user.Login,
            Password = "test",
            Role = _user.Role
        });
    }

    [Fact]
    public async Task CreateBookingTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task CreateMultipleBookingTest()
    {
        var bookingFirst = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
        var bookingSecond = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);

        Assert.NotNull(bookingFirst);
        Assert.NotNull(bookingSecond);
        Assert.NotEqual(bookingFirst.Id, bookingSecond.Id);
    }

    [Fact]
    public async Task GetBookingByIdTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);

        Assert.NotNull(booking);

        var bookingFromStorage = await _bookingService.GetBookingByIdAsync(booking.Id);
        Assert.Equal(booking.Id, bookingFromStorage?.Id);
    }

     [Fact]
     public async Task VerifyBookingStatusChangeTest()
     {
         var availableSeatsBeforeBooking = _testEvent.AvailableSeats;
    
         var eventService = new Mock<IEventService>();
         eventService.Setup(r =>  r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var realBookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
         var pendingBooking = await realBookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
    
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
         await cts.CancelAsync();
    
         Assert.NotNull(pendingBooking);
         Assert.Equal(BookingStatus.Confirmed, pendingBooking.Status);
         Assert.NotNull(pendingBooking.ProcessedAt);
         Assert.Equal(_testEvent.AvailableSeats, availableSeatsBeforeBooking - 1);
     }

     [Fact]
     public async Task VerifyEventNotFoundTriggersBookingRejectTest()
     {
         var eventService = new Mock<IEventService>();
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var realBookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
         var pendingBooking = await realBookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
    
         var mockServiceProvider = new Mock<IServiceProvider>();
         mockServiceProvider
             .Setup(p => p.GetService(typeof(IBookingService)))
             .Returns(realBookingService);
    
         var mockServiceScope = new Mock<IServiceScope>();
         mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
    
         var mockScopeFactory = new Mock<IServiceScopeFactory>();
         mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
    
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id))
             .Throws(new EventNotFoundException($"Не удалось найти событие с идентификатором {_testEvent.Id}"));
    
         var mockLogger = new Mock<ILogger<BookingWorker>>();
         var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);
    
         using var cts = new CancellationTokenSource();
    
         await worker.StartAsync(cts.Token);
    
         await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
         await cts.CancelAsync();
    
         Assert.NotNull(pendingBooking);
         Assert.Equal(BookingStatus.Rejected, pendingBooking.Status);
         Assert.NotNull(pendingBooking.ProcessedAt);
     }

     [Fact]
     public async Task VerifyBookingRejectTriggersReleaseSeatsTest()
     {
         var availableSeats = _testEvent.AvailableSeats;
    
         var eventService = new Mock<IEventService>();
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var realBookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
         var pendingBooking = await realBookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
    
         var mockServiceProvider = new Mock<IServiceProvider>();
         mockServiceProvider
             .Setup(p => p.GetService(typeof(IBookingService)))
             .Returns(realBookingService);
    
         var mockServiceScope = new Mock<IServiceScope>();
         mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
    
         var mockScopeFactory = new Mock<IServiceScopeFactory>();
         mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
    
         eventService.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Throws(new System.Exception());
    
         var mockLogger = new Mock<ILogger<BookingWorker>>();
         var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);
    
         using var cts = new CancellationTokenSource();
    
         await worker.StartAsync(cts.Token);
    
         await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
         await cts.CancelAsync();
    
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
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var realBookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
         var pendingBooking = await realBookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
    
         var mockServiceProvider = new Mock<IServiceProvider>();
         mockServiceProvider
             .Setup(p => p.GetService(typeof(IBookingService)))
             .Returns(realBookingService);
    
         var mockServiceScope = new Mock<IServiceScope>();
         mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
    
         var mockScopeFactory = new Mock<IServiceScopeFactory>();
         mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
    
         eventService.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Throws(new System.Exception());
    
         var mockLogger = new Mock<ILogger<BookingWorker>>();
         var worker = new BookingWorker(mockLogger.Object, mockScopeFactory.Object);
    
         using var cts = new CancellationTokenSource();
    
         await worker.StartAsync(cts.Token);
    
         await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
         await cts.CancelAsync();
    
         Assert.NotNull(pendingBooking);
         Assert.Equal(BookingStatus.Rejected, pendingBooking.Status);
         Assert.NotNull(pendingBooking.ProcessedAt);
         Assert.Equal(_testEvent.AvailableSeats, availableSeats);
    
         var secondPendingBooking = await realBookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
    
         eventService.Setup(r => r.UpdateAsync(It.IsAny<Event>())).ReturnsAsync(It.IsAny<EventInfoDto>());
         using var ctsNew = new CancellationTokenSource();
    
         await worker.StartAsync(ctsNew.Token);
    
         await Task.Delay(TimeSpan.FromSeconds(4), ctsNew.Token);
         await ctsNew.CancelAsync();
    
         Assert.NotNull(secondPendingBooking);
         Assert.Equal(BookingStatus.Confirmed, secondPendingBooking.Status);
         Assert.NotNull(secondPendingBooking.ProcessedAt);
     }

    [Fact]
    public async Task CreateNonExistingEventBookingTest()
    {
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(Guid.NewGuid(), _user.Id));
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

        await _eventService.CreateAsync(eventItem);
        await _eventService.DeleteAsync(eventId);
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(eventId, _user.Id));
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
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);

        Assert.NotNull(booking);
        Assert.Equal(_testEvent.AvailableSeats, seatsBeforeBooking - 1);
    }

    [Fact]
    public async Task BookingCreateWithUniqueIds()
    {
        var firstBooking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
        var secondBooking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);

        Assert.NotNull(firstBooking);
        Assert.NotNull(secondBooking);

        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
    }

    [Fact]
    public async Task NotAvailableSeatsThrowTest()
    {
        await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
        await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id));
    }

    [Fact]
    public async Task BookingWithNoAvailableSeats()
    {
        _testEvent.AvailableSeats = 0;
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id));
    }

    [Fact]
    public async Task BookingChangeStatusMethodsTest()
    {
        var booking = await _bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);

        Assert.NotNull(booking);

        booking.Confirm();
        Assert.Equal(BookingStatus.Confirmed, booking.Status);

        booking.Reject();
        Assert.Equal(BookingStatus.Rejected, booking.Status);
    }

     [Fact]
     public async Task ConcurrencyAvailableSeatsTest()
     {
         _testEvent.AvailableSeats = 5;
         var eventService = new Mock<IEventService>();
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var bookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
    
         using var cts = new CancellationTokenSource();
    
         var exceptionBag = new ConcurrentBag<NoAvailableSeatsException>();
         var pendingBooking = new ConcurrentBag<Booking>();
    
         var tasks = new List<Task>();
         for (var i = 0; i < 20; i++)
         {
             tasks.Add(Task.Run(async () =>
             {
                 try
                 {
                     var booking = await bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
                     pendingBooking.Add(booking);
                 }
                 catch (NoAvailableSeatsException e)
                 {
                     exceptionBag.Add(e);
                 }
             }, cts.Token));
         }
    
         await Task.WhenAll(tasks);
    
         Assert.NotEmpty(exceptionBag);
         Assert.Equal(15, exceptionBag.Count);
         Assert.Equal(0, _testEvent.AvailableSeats);
         Assert.Equal(5, pendingBooking.Count);
     }
    
     [Fact]
     public async Task ConcurrencyUniqueBookingIdsTest()
     {
         _testEvent.AvailableSeats = 10;
         var eventService = new Mock<IEventService>();
         eventService.Setup(r => r.GetEntityByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
    
         var bookingLogger = new Logger<BookingService>(new LoggerFactory());
    
         var bookingService = new BookingService(new BookingRepository(_dbContext), bookingLogger, eventService.Object, _userService);
    
         using var cts = new CancellationTokenSource();
    
         var exceptionBag = new ConcurrentBag<NoAvailableSeatsException>();
         var pendingBooking = new ConcurrentBag<Booking>();
    
         var tasks = new List<Task>();
         for (var i = 0; i < 10; i++)
         {
             tasks.Add(Task.Run(async () =>
             {
                 try
                 {
                     var booking = await bookingService.CreateBookingAsync(_testEvent.Id, _user.Id);
                     pendingBooking.Add(booking);
                 }
                 catch (NoAvailableSeatsException e)
                 {
                     exceptionBag.Add(e);
                 }
             }, cts.Token));
         }
    
         await Task.WhenAll(tasks);
         Assert.Empty(exceptionBag);
         var uniqueBookingIds = pendingBooking.Select(s => s.Id).Distinct();
         Assert.Equal(10, uniqueBookingIds.Count());
     }
     
     [Fact]
     public async Task BookAlreadyPassedEventTest()
     {
         var passedEvent = new Event
         {
             Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
             Title = "Прошедший эвент",
             Description = "Описание события",
             StartAt = new DateTime(2025, 01, 01),
             EndAt = new DateTime(2025, 01, 31),
             TotalSeats = 2,
             AvailableSeats = 2
         };
         
         await _eventService.CreateAsync(passedEvent);

         await Assert.ThrowsAsync<EventExpiredException>(() =>
             _bookingService.CreateBookingAsync(passedEvent.Id, _user.Id));
     }
     
     [Fact]
     public async Task BookingLimitExceededTest()
     {
         var testEvent = new Event
         {
             Id = new Guid("1a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
             Title = "Прошедший эвент",
             Description = "Описание события",
             StartAt = new DateTime(2030, 01, 01),
             EndAt = new DateTime(2030, 01, 31),
             TotalSeats = 20,
             AvailableSeats = 20
         };
         
         await _eventService.CreateAsync(testEvent);

         foreach (var _ in Enumerable.Range(0, 10))
         {
             await _bookingService.CreateBookingAsync(testEvent.Id, _user.Id);
             await Task.Delay(1000); 
         }
         
         await Assert.ThrowsAsync<BookingLimitExceededException>(() =>
             _bookingService.CreateBookingAsync(testEvent.Id, _user.Id));
     }
}