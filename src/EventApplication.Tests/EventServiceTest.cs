using EventApplication.Exception;
using EventApplication.Models;
using EventApplication.Service.Implementation;
using EventApplication.Service.Interface;
using Xunit;

namespace EventApplication.Tests;

public class EventServiceTest
{
    private readonly IEventService _eventService;
    private readonly Event _testEvent;
    
    public EventServiceTest()
    {
        _eventService = new EventService();
        
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
    public void CreateEventTest()
    {
        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        var createdDto = _eventService.Create(eventToCreate);
        Assert.Equal(eventToCreate.Id, createdDto.Id);
    }

    [Fact]
    public void GetAllEventsTest()
    {
        var eventList = _eventService.GetAll();
        var eventsCount = eventList.Data.Count();
        Assert.Equal(1, eventsCount);
    }

    [Theory]
    [InlineData( "8a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b")]
    public void GetEventByIdTest(string id)
    {
        var eventItem = _eventService.GetById(new Guid(id));
        Assert.Equal(_testEvent.Id, eventItem.Id);
    }
    
    [Fact]
    public void CreateWithExisingIdTest()
    {
        Assert.Throws<EventAlreadyExistsException>(() => _eventService.Create(_testEvent));
    }
    
    [Fact]
    public void UpdateEventTest()
    {
        var eventToUpdate = _eventService.GetById(_testEvent.Id);
        eventToUpdate.Title = "updated title";
        var updatedEvent = _eventService.Update(Mapper.EventMapper.MapToEvent(eventToUpdate));
        Assert.Equal(updatedEvent.Title,  eventToUpdate.Title);
    }
    
    [Fact]
    public void DeleteEventTest()
    {
        _eventService.Delete(_testEvent.Id);
        var eventList = _eventService.GetAll();
        var eventsCount = eventList.Data.Count();
        Assert.Equal(0, eventsCount);
    }
    
    [Fact]
    public void DeleteNonExisingIdTest()
    {
        Assert.Throws<EventNotFoundException>(() => _eventService.Delete(new  Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b")));
    }
    
    [Fact]
    public void FilterByTitleTest()
    {
        var eventByTitle = _eventService.GetAll("Заголовок события").Data?.FirstOrDefault();
        Assert.Equal(_testEvent.Id, eventByTitle?.Id);
    }
    
    [Fact]
    public void FilterByDatesTest()
    {
        var eventByDates = _eventService.GetAll(
            from: new DateTime(2019, 01, 02), 
            to: new DateTime(2020, 02, 15)).Data?.FirstOrDefault();
        Assert.Equal(_testEvent.Id, eventByDates?.Id);
    }
    
    [Fact]
    public void FilterByDatesNoResultTest()
    {
        var eventByDates = _eventService.GetAll(
            from: new DateTime(2099, 01, 02), 
            to: new DateTime(2100, 02, 15)).Data?.FirstOrDefault();
        Assert.Null(eventByDates);
    }
    
    [Fact]
    public void FilterByDateFromTest()
    {
        var eventByDates = _eventService.GetAll(
            from: new DateTime(2019, 01, 02)).Data?.FirstOrDefault();
        Assert.Equal(eventByDates?.Id, _testEvent.Id);
    }
    
    [Fact]
    public void FilterByDateToTest()
    {
        var eventByDates = _eventService.GetAll(
            to: new DateTime(2024, 01, 02)).Data?.FirstOrDefault();
        Assert.Equal(eventByDates?.Id, _testEvent.Id);
    }
    
    [Theory]
    [InlineData(1, 2, null, 2)]
    [InlineData(1, 1, null, 1)]
    [InlineData(1, 1, "Заголовок события", 1)]
    [InlineData(1, 1, "", 1)]
    public void FilterPaginationTest(int? page, int? pageSize, string? title, int expectedCount)
    {
        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        _eventService.Create(eventToCreate);

        var getByPaginationCount =_eventService.GetAll(title: title, page: page, pageSize: pageSize).Data?.Count();
        Assert.Equal(expectedCount, getByPaginationCount);
    }
    
    [Fact]
    public void SearchNonExistingIdTest()
    {
        var id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b");
        Assert.Throws<EventNotFoundException>(() => _eventService.GetById(id));
    }
    
    [Fact]
    public void UpdateNonExistingIdTest()
    {
        var eventToUpdate = new Event
        {
            Id = new Guid("1a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01),
            EndAt = new DateTime(2020, 01, 31),
        };
        
        Assert.Throws<EventNotFoundException>(() => _eventService.Update(eventToUpdate));
    }
}