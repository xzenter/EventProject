using EventProject.DataAccess;
using EventProject.Dto.Query;
using EventProject.Exceptions;
using EventProject.Services.Booking;
using EventProject.Services.Event;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Tests;

public class EventServiceInMemoryTests
{
    private readonly ServiceProvider _serviceProvider;

    public EventServiceInMemoryTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        // Регистрируем сервисы
        services.AddScoped<EventService>();
        services.AddScoped<BookingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private readonly List<EventForCreationQuery> _eventsForCreation =
    [
        new()
        {
            Title = "Test Event 1",
            Description = "Test Description 1",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        },

        new()
        {
            Title = "Test Event 2",
            Description = "Test Description 2",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(2)
        },

        new()
        {
            Title = "Test Event 3",
            Description = "Test Description 3",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(3)
        },

        new()
        {
            Title = "Test Event 4",
            Description = "Test Description 4",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(4)
        }
    ];

    // создание события
    [Fact]
    public async Task CreateEvent_ShouldReturnsEvent()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var eventForCreation = new EventForCreationQuery
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var eventDto = await eventService.CreateEvent(eventForCreation);

        eventDto.Should().NotBeNull();
        eventDto.Title.Should().Be(eventForCreation.Title);
        eventDto.Description.Should().Be(eventForCreation.Description);
        eventDto.StartAt.Should().Be(eventForCreation.StartAt);
        eventDto.EndAt.Should().Be(eventForCreation.EndAt);
    }


    // получение всех событий
    [Fact]
    public async Task GetEvents_ShouldReturnsEvents()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);


        var searchQuery = new SearchEventsQuery
        {
            Title = null,
            From = null,
            To = null,
            Page = 1,
            PageSize = _eventsForCreation.Count
        };

        var result = await eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(_eventsForCreation.Count);
        result.TotalItems.Should().Be(_eventsForCreation.Count);
    }

    // получение события по ID
    [Fact]
    public async Task GetEventById_ShouldReturnsEvent()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var eventForCreation = _eventsForCreation[0];
        var eventDto = await eventService.CreateEvent(eventForCreation);

        var result = await eventService.GetEvent(eventDto.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventDto.Id);
        result.Title.Should().Be(eventForCreation.Title);
        result.Description.Should().Be(eventForCreation.Description);
        result.StartAt.Should().Be(eventForCreation.StartAt);
        result.EndAt.Should().Be(eventForCreation.EndAt);
    }

    // обновление существующего события
    [Fact]
    public async Task UpdateEvent_ShouldReturnsUpdatedEvent()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var eventForCreation = _eventsForCreation[0];
        var eventDto = await eventService.CreateEvent(eventForCreation);

        var eventForUpdate = new EventForUpdateQuery
        {
            Title = "New Title",
            Description = "New Description",
            StartAt = DateTime.Now.AddDays(10),
            EndAt = DateTime.Now.AddDays(20)
        };

        var result = await eventService.UpdateEvent(eventDto.Id, eventForUpdate);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventDto.Id);
        result.Title.Should().Be(eventForUpdate.Title);
        result.Description.Should().Be(eventForUpdate.Description);
        result.StartAt.Should().Be(eventForUpdate.StartAt);
        result.EndAt.Should().Be(eventForUpdate.EndAt);
    }

    // удаление существующего события
    [Fact]
    public async Task DeleteEvent_ShouldDeleteEvent()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var eventForCreation = _eventsForCreation[0];
        var eventDto = await eventService.CreateEvent(eventForCreation);

        await eventService.DeleteEvent(eventDto.Id);

        var exception = () => eventService.GetEvent(eventDto.Id);

        await exception.Should().ThrowAsync<NotFoundException>();
    }

    // фильтрация по названию
    [Fact]
    public async Task GetEvents_ShouldReturnsFilteredEvents()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);

        var searchQuery = new SearchEventsQuery
        {
            Title = "Test Event 2",
            From = null,
            To = null,
            Page = 1,
            PageSize = 10
        };

        var result = await eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        result.Items.First().Title.Should().Be(searchQuery.Title);
    }

    // фильтрация по датам (startDate, endDate)
    [Fact]
    public async Task GetEvents_ShouldReturnsFilteredEventsByDates()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);

        var event1 = _eventsForCreation.First();

        var searchQuery = new SearchEventsQuery
        {
            // Title = null,
            From = event1.StartAt,
            To = event1.EndAt,
            Page = 1,
            PageSize = 10
        };

        var result = await eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        result.Items.First().Title.Should().Be(event1.Title);
        result.Items.First().StartAt.Should().Be(event1.StartAt);
        result.Items.First().EndAt.Should().Be(event1.EndAt);
    }

    // пагинация событий
    [Theory]
    [InlineData(1, 10, 4)]
    [InlineData(2, 10, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, 1)]
    [InlineData(5, 1, 0)]
    public async Task GetEvents_ShouldReturnsPaginatedResult(int page, int pageSize, int expectedCount)
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);

        var searchQuery = new SearchEventsQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(expectedCount);
        result.TotalItems.Should().Be(_eventsForCreation.Count);
        result.Page.Should().Be(searchQuery.Page);
        result.PageSize.Should().Be(searchQuery.PageSize);
    }

    // комбинированная фильтрация
    [Fact]
    public async Task GetEvents_ShouldReturnsFilteredAndPaginatedResult()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);

        var element1 = _eventsForCreation.First();
        var element2 = _eventsForCreation.Skip(1).First();

        var searchQuery = new SearchEventsQuery
        {
            Title = "Test",
            From = element1.StartAt,
            To = element2.EndAt,
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    // попытка получить событие с несуществующим ID
    [Fact]
    public async Task GetEvent_ShouldThrowNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var exception = () => eventService.GetEvent(Guid.NewGuid());

        await exception.Should().ThrowAsync<NotFoundException>();
    }

    // попытка обновить событие с несуществующим ID
    [Fact]
    public async Task UpdateEvent_ShouldThrowNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var newDto = new EventForUpdateQuery
        {
            Title = "Test",
            Description = "Test description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var exception = () => eventService.UpdateEvent(Guid.NewGuid(), newDto);

        await exception.Should().ThrowAsync<NotFoundException>();
    }

    // создание события с некорректными данными (если валидация в сервисе)
    [Fact]
    public async Task CreateEvent_EmptyTitle_ShouldThrowBadRequestException()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        var newDto = new EventForCreationQuery
        {
            Title = null!,
            Description = null,
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var exception = () => eventService.CreateEvent(newDto);

        await exception.Should().ThrowAsync<BadRequestException>();
    }

    // обновление события с некорректными датами (EndAt раньше StartAt)
    [Fact]
    public async Task UpdateEvent_EndAtBeforeStartAt_ShouldThrowBadRequestException()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();

        foreach (var eventForCreation in _eventsForCreation) await eventService.CreateEvent(eventForCreation);

        var events = await eventService.GetEvents(new SearchEventsQuery());
        var event1 = events.Items.First();

        var newEvent = new EventForUpdateQuery
        {
            Title = "Test",
            Description = "Test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(-1)
        };

        var exception = () => eventService.UpdateEvent(event1.Id, newEvent);

        await exception.Should().ThrowAsync<BadRequestException>();
    }
}