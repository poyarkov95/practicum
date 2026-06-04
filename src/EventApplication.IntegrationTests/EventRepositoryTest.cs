using Domain.Entities;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventApplication.IntegrationTests;

public class EventRepositoryTest : DatabaseTestManager
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
    public async Task GetByIdTest()
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await using var verifyContext = await CreateContext();
        var repository = new EventRepository(verifyContext);
        var result = await repository.GetByIdAsync(_testEvent.Id);
        
        //Assert
        Assert.Equal(_testEvent.Id, result?.Id);
    }

    [Theory]
    [InlineData(1, 2, null, null, null, 2)]
    [InlineData(1, 1, null, null, null, 1)]
    [InlineData(1, 10, "ловок события", null, null, 2)]
    [InlineData(1, 10, "Заголовок события", null, null, 2)]
    [InlineData(1, 10, "ЗаГолОвОк событиЯ", null, null, 2)]
    [InlineData(1, 1, "", null, null, 1)]
    [InlineData(1, 1, "", "2019-01-01", "2022-01-31", 1)]
    [InlineData(1, 1, "", "2023-01-01", null, 0)]
    [InlineData(1, 1, "", null, "2023-01-01", 1)]
    [InlineData(1, 1, "", "2021-01-01", "2023-01-01", 0)]
    [InlineData(1, 10, "Заголовок события", "2019-01-01", "2022-01-31", 2)]
    public async Task GetAllAsyncTest(int? page, int? pageSize, string? title, string? from, string? to,
        int expectedCount)
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);

        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01).ToUniversalTime(),
            EndAt = new DateTime(2020, 01, 31).ToUniversalTime(),
        };
        context.Events.Add(eventToCreate);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await using var getContext = await CreateContext();
        var repository = new EventRepository(getContext);

        DateTime? fromParsed = null;
        DateTime? toParsed = null;
        
        if (!string.IsNullOrEmpty(from))
        {
            fromParsed = DateTime.Parse(from).ToUniversalTime();
        }
        
        if (!string.IsNullOrEmpty(to))
        {
            toParsed = DateTime.Parse(to).ToUniversalTime();
        }

        var getByPaginationCount = (await repository.GetAllAsync(title: title, from: fromParsed, to: toParsed, page: page, pageSize: pageSize)).Count;
        
        //Assert
        Assert.Equal(expectedCount, getByPaginationCount);
    }
    
    [Theory]
    [InlineData(1, 2, null, null, null, 2)]
    [InlineData(1, 1, null, null, null, 2)]
    [InlineData(1, 10, "ловок события", null, null, 2)]
    [InlineData(1, 10, "Заголовок события", null, null, 2)]
    [InlineData(1, 10, "ЗаГолОвОк событиЯ", null, null, 2)]
    [InlineData(1, 1, "", null, null, 2)]
    [InlineData(1, 1, "", "2019-01-01", "2022-01-31", 2)]
    [InlineData(1, 1, "", "2023-01-01", null, 0)]
    [InlineData(1, 1, "", null, "2023-01-01", 2)]
    [InlineData(1, 1, "", "2021-01-01", "2023-01-01", 0)]
    [InlineData(1, 10, "Заголовок события", "2019-01-01", "2022-01-31", 2)]
    public async Task CountTest(int? page, int? pageSize, string? title, string? from, string? to,
        int expectedCount)
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);

        var eventToCreate = new Event
        {
            Id = new Guid("9a1f2e3b4c5d6e7f8a9b0c1d2e3f4a5b"),
            Title = "Заголовок события",
            Description = "Описание события",
            StartAt = new DateTime(2020, 01, 01).ToUniversalTime(),
            EndAt = new DateTime(2020, 01, 31).ToUniversalTime(),
        };
        context.Events.Add(eventToCreate);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await using var getContext = await CreateContext();
        var repository = new EventRepository(getContext);

        DateTime? fromParsed = null;
        DateTime? toParsed = null;
        
        if (!string.IsNullOrEmpty(from))
        {
            fromParsed = DateTime.Parse(from).ToUniversalTime();
        }
        
        if (!string.IsNullOrEmpty(to))
        {
            toParsed = DateTime.Parse(to).ToUniversalTime();
        }

        var getByPaginationCount = (await repository.CountAsync(title: title, from: fromParsed, to: toParsed, page: page, pageSize: pageSize));
        
        //Assert
        Assert.Equal(expectedCount, getByPaginationCount);
    }
    
    [Fact]
    public async Task CreateEventTest()
    {
        // Arrange
        await using var context = await CreateContext();
        var repository = new EventRepository(context);
        await repository.CreateAsync(_testEvent);

        //Act
        await using var verifyContext = await CreateContext();
        var result = await verifyContext.Events.FirstOrDefaultAsync(s => s.Id == _testEvent.Id, cancellationToken: TestContext.Current.CancellationToken)!;
        
        //Assert
        Assert.NotNull(_testEvent);
        Assert.Equal(_testEvent.Id, result?.Id);
    }
    
    [Fact]
    public async Task UpdateEventTest()
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await using var verifyContext = await CreateContext();
        var repository = new EventRepository(verifyContext);
        var eventItem = await repository.GetByIdAsync(_testEvent.Id);
        eventItem!.Description = "Новое описание";
        await repository.UpdateAsync(eventItem);
        
        await using var getContext = await CreateContext();
        var result = await getContext.Events.FirstOrDefaultAsync(s => s.Id == _testEvent.Id, cancellationToken: TestContext.Current.CancellationToken)!;
        
        //Assert
        Assert.Equal(_testEvent.Id, result?.Id);
        Assert.Equal("Новое описание", result.Description);
    }
    
    [Fact]
    public async Task DeleteEventTest()
    {
        // Arrange
        await using var context = await CreateContext();
        await context.Events.AddAsync(_testEvent, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await using var verifyContext = await CreateContext();
        var repository = new EventRepository(verifyContext);
        await repository.DeleteAsync(_testEvent);
        
        await using var getContext = await CreateContext();
        var result = await getContext.Events.FirstOrDefaultAsync(s => s.Id == _testEvent.Id, cancellationToken: TestContext.Current.CancellationToken)!;
        
        //Assert
        Assert.Null(result);
    }
}