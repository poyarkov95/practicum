using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Implementation;
using Application.Abstractions.Services.Interface;
using Common.Settings;
using Confluent.Kafka;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests;

public class CacheServiceTest
{
    private readonly IEventService _eventService;
    private readonly Mock<IEventRepository> _eventRepository;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Event _testEvent;
    
    public CacheServiceTest()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        
        var mockProducer = new Mock<IProducer<Null, string>>();
        services.AddSingleton(mockProducer.Object);
        
        _eventRepository = new Mock<IEventRepository>();
        services.AddSingleton(_eventRepository.Object);
        
        services.AddScoped<IEventProducer, EventProducer>();
        
        _cacheService = new Mock<ICacheService>();
        services.AddScoped<ICacheService>(_ => _cacheService.Object);
        
        services.AddScoped<IEventService, EventService>();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["CacheSettings:EventCacheTTLSeconds"] = "300",
                ["CacheSettings:TopEventsCacheTTLSeconds"] = "600",
                ["KafkaConfiguration:BootstrapServers"] = "localhost:9092"
            }!)
            .Build();
        
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
        
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
    }
    
    [Fact]
    public async Task GetDataFromCacheTest()
    {
        _cacheService
            .Setup(x => x.GetAsync<Event>($"event:{_testEvent.Id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testEvent);
        var eventItem = await _eventService.GetByIdAsync(_testEvent.Id, CancellationToken.None);
        _eventRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }
    
    [Fact]
    public async Task GetDataFromRepositoryTest()
    {
        _eventRepository
            .Setup(x => x.CreateAsync(
                It.Is<Event>(e => e.Title == _testEvent.Title && e.Description == _testEvent.Description)))
            .ReturnsAsync(_testEvent);
        
        Task.Run(() => _eventService.CreateAsync(_testEvent)).GetAwaiter().GetResult();
        
        _cacheService
            .Setup(x => x.GetAsync<Event>($"event:{_testEvent.Id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Task.FromResult<Event?>(null).Result);
        
        _eventRepository
            .Setup(x => x.GetByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
        
        var eventItem = await _eventService.GetByIdAsync(_testEvent.Id, CancellationToken.None);
        _eventRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.AtLeast(2));
    }
    
    [Fact]
    public async Task InvalidateCacheTest()
    {
        _cacheService
            .Setup(x => x.GetAsync<Event>($"event:{_testEvent.Id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Task.FromResult<Event?>(null).Result);
        
        _eventRepository
            .Setup(x => x.GetByIdAsync(_testEvent.Id)).ReturnsAsync(_testEvent);
        
        _eventRepository
            .Setup(x => x.UpdateAsync(_testEvent)).ReturnsAsync(_testEvent);

        await _eventService.UpdateAsync(_testEvent);
        _cacheService.Verify(
            x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None),
            Times.AtLeast(1));
    }
}