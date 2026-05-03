using EventApplication.Database;
using EventApplication.Exception;
using EventApplication.Models;
using EventApplication.Service.Implementation;
using EventApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EventApplication.Tests;

public class EventServiceTest
{
    private readonly IEventService _eventService;
    private readonly Event _testEvent;
    
    public EventServiceTest()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        
        services.AddLogging(configure => configure.AddConsole());
        
        var serviceProvider = services.BuildServiceProvider(); 
        var scope = serviceProvider.CreateScope();
        _eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        
        _testEvent = new Event
        {
            Id = new Guid("8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31)
        };
        
        Task.Run(() => _eventService.CreateAsync(_testEvent)).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateEventTest()
    {
        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        var createdDto = await _eventService.CreateAsync(eventToCreate);
        Assert.Equal(eventToCreate.Id, createdDto.Id);
    }
    
    [Fact]
    public async Task GetAllEventsTest()
    {
        var eventList = await _eventService.GetAllAsync();
        var eventsCount = eventList.Data.Count();
        Assert.Equal(1, eventsCount);
    }
    
    [Theory]
    [InlineData( "8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b")]
    public async Task GetEventByIdTest(string id)
    {
        var eventItem = await _eventService.GetByIdAsync(new Guid(id));
        Assert.Equal(_testEvent.Id, eventItem.Id);
    }
    
    [Fact]
    public async Task CreateWithExisingIdTest()
    {
        await Assert.ThrowsAsync<EventAlreadyExistsException>(() => _eventService.CreateAsync(_testEvent));
    }
    
    [Fact]
    public async Task UpdateEventTest()
    {
        var eventToUpdate = await _eventService.GetEntityByIdAsync(_testEvent.Id);
        eventToUpdate.Title = "updated title";
        var updatedEvent = await _eventService.UpdateAsync(eventToUpdate);
        Assert.Equal(updatedEvent.Title,  eventToUpdate.Title);
    }
    
    [Fact]
    public async Task DeleteEventTest()
    {
        await _eventService.DeleteAsync(_testEvent.Id);
        var eventList = await _eventService.GetAllAsync();
        var eventsCount = eventList.Data.Count();
        Assert.Equal(0, eventsCount);
    }
    
    [Fact]
    public async Task DeleteNonExisingIdTest()
    {
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.DeleteAsync(new  Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b")));
    }
    
    [Fact]
    public async Task FilterByTitleTest()
    {
        var eventByTitle = (await _eventService.GetAllAsync("Заголовок события")).Data?.FirstOrDefault();
        Assert.Equal(_testEvent.Id, eventByTitle?.Id);
        
        var eventBySubTitle = (await _eventService.GetAllAsync("овок события")).Data?.FirstOrDefault();
        Assert.Equal(_testEvent.Id, eventByTitle?.Id);
    }
    
    [Fact]
    public async Task FilterByDatesTest()
    {
        var eventByDates = (await _eventService.GetAllAsync(
            from: new DateTime(2019, 01, 02), 
            to: new DateTime(2020, 02, 15))).Data?.FirstOrDefault();
        Assert.Equal(_testEvent.Id, eventByDates?.Id);
    }
    
    [Fact]
    public async Task FilterByDatesNoResultTest()
    {
        var eventByDates = (await _eventService.GetAllAsync(
            from: new DateTime(2099, 01, 02), 
            to: new DateTime(2100, 02, 15))).Data?.FirstOrDefault();
        Assert.Null(eventByDates);
    }
    
    [Fact]
    public async Task FilterByDateFromTest()
    {
        var eventByDates = (await _eventService.GetAllAsync(
            from: new DateTime(2019, 01, 02))).Data?.FirstOrDefault();
        Assert.Equal(eventByDates?.Id, _testEvent.Id);
    }
    
    [Fact]
    public async Task FilterByDateToTest()
    {
        var eventByDates = (await _eventService.GetAllAsync(
            to: new DateTime(2024, 01, 02))).Data?.FirstOrDefault();
        Assert.Equal(eventByDates?.Id, _testEvent.Id);
    }
    
    [Fact]
    public async Task FilterByAllParametersTest()
    {
        var eventByDates = (await _eventService.GetAllAsync(
            page: 1,
            pageSize: 10,
            title: "заг", 
            from: new DateTime(2019, 01, 02),
            to: new DateTime(2024, 01, 02)
            )).Data?.FirstOrDefault();
        
        Assert.Equal(eventByDates?.Id, _testEvent.Id);
    }
    
    [Theory]
    [InlineData(1, 2, null, 2)]
    [InlineData(1, 1, null, 1)]
    [InlineData(1, 1, "ловок события", 1)]
    [InlineData(1, 1, "Заголовок события", 1)]
    [InlineData(1, 1, "ЗаГолОвОк событиЯ", 1)]
    [InlineData(1, 1, "", 1)]
    public async Task FilterPaginationTest(int? page, int? pageSize, string? title, int expectedCount)
    {
        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        _eventService.CreateAsync(eventToCreate);
    
        var getByPaginationCount = (await _eventService.GetAllAsync(title: title, page: page, pageSize: pageSize)).Data?.Count();
        Assert.Equal(expectedCount, getByPaginationCount);
    }
    
    [Fact]
    public async Task SearchNonExistingIdTest()
    {
        var id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b");
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.GetByIdAsync(id));
    }
    
    [Fact]
    public async Task UpdateNonExistingIdTest()
    {
        var eventToUpdate = new Event
        {
            Id = new Guid("1a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.UpdateAsync(eventToUpdate));
    }
}