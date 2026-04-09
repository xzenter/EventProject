using EventProject.Controllers.Events.Query;
using EventProject.Exceptions;
using EventProject.Repository;
using EventProject.Services;
using FluentAssertions;

namespace EventProject.Tests;

public class EventServiceTests
{
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventService = new EventService(new EventRepository());
    }

    private readonly List<EventForCreationQuery> _eventsForCreation =
    [
        new EventForCreationQuery
        {
            Title = "Test Event 1",
            Description = "Test Description 1",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        },

        new EventForCreationQuery
        {
            Title = "Test Event 2",
            Description = "Test Description 2",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(2)
        },

        new EventForCreationQuery
        {
            Title = "Test Event 3",
            Description = "Test Description 3",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(3)
        },

        new EventForCreationQuery
        {
            Title = "Test Event 4",
            Description = "Test Description 4",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(4)
        }
    ];

    // создание события
    [Fact]
    public void CreateEvent_ShouldReturnsEvent()
    {
        var eventForCreation = new EventForCreationQuery
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var eventDto = _eventService.CreateEvent(eventForCreation);

        eventDto.Should().NotBeNull();
        eventDto.Title.Should().Be(eventForCreation.Title);
        eventDto.Description.Should().Be(eventForCreation.Description);
        eventDto.StartAt.Should().Be(eventForCreation.StartAt);
        eventDto.EndAt.Should().Be(eventForCreation.EndAt);
    }


    // получение всех событий
    [Fact]
    public void GetEvents_ShouldReturnsEvents()
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }

        var searchQuery = new SearchEventsQuery
        {
            Title = null,
            From = null,
            To = null,
            Page = 1,
            PageSize = _eventsForCreation.Count
        };

        var result = _eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(_eventsForCreation.Count);
        result.TotalItems.Should().Be(_eventsForCreation.Count);
    }

    // получение события по ID
    [Fact]
    public void GetEventById_ShouldReturnsEvent()
    {
        var eventForCreation = _eventsForCreation[0];
        var eventDto = _eventService.CreateEvent(eventForCreation);

        var result = _eventService.GetEvent(eventDto.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventDto.Id);
        result.Title.Should().Be(eventForCreation.Title);
        result.Description.Should().Be(eventForCreation.Description);
        result.StartAt.Should().Be(eventForCreation.StartAt);
        result.EndAt.Should().Be(eventForCreation.EndAt);
    }

    // обновление существующего события
    [Fact]
    public void UpdateEvent_ShouldReturnsUpdatedEvent()
    {
        var eventForCreation = _eventsForCreation[0];
        var eventDto = _eventService.CreateEvent(eventForCreation);

        var eventForUpdate = new EventForUpdateQuery
        {
            Title = "New Title",
            Description = "New Description",
            StartAt = DateTime.Now.AddDays(10),
            EndAt = DateTime.Now.AddDays(20)
        };

        var result = _eventService.UpdateEvent(eventDto.Id, eventForUpdate);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventDto.Id);
        result.Title.Should().Be(eventForUpdate.Title);
        result.Description.Should().Be(eventForUpdate.Description);
        result.StartAt.Should().Be(eventForUpdate.StartAt);
        result.EndAt.Should().Be(eventForUpdate.EndAt);
    }

    // удаление существующего события
    [Fact]
    public void DeleteEvent_ShouldDeleteEvent()
    {
        var eventForCreation = _eventsForCreation[0];
        var eventDto = _eventService.CreateEvent(eventForCreation);

        _eventService.DeleteEvent(eventDto.Id);

        var exception = () => _eventService.GetEvent(eventDto.Id);

        exception.Should().Throw<NotFoundException>();
    }

    // фильтрация по названию
    [Fact]
    public void GetEvents_ShouldReturnsFilteredEvents()
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }

        var searchQuery = new SearchEventsQuery
        {
            Title = "Test Event 2",
            From = null,
            To = null,
            Page = 1,
            PageSize = 10
        };

        var result = _eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        result.Items.First().Title.Should().Be(searchQuery.Title);
    }

    // фильтрация по датам (startDate, endDate)
    [Fact]
    public void GetEvents_ShouldReturnsFilteredEventsByDates()
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }

        var event1 = _eventsForCreation.First();

        var searchQuery = new SearchEventsQuery
        {
            // Title = null,
            From = event1.StartAt,
            To = event1.EndAt,
            Page = 1,
            PageSize = 10
        };

        var result = _eventService.GetEvents(searchQuery);

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
    public void GetEvents_ShouldReturnsPaginatedResult(int page, int pageSize, int expectedCount)
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }

        var searchQuery = new SearchEventsQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = _eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(expectedCount);
        result.TotalItems.Should().Be(_eventsForCreation.Count);
        result.Page.Should().Be(searchQuery.Page);
        result.PageSize.Should().Be(searchQuery.PageSize);
    }

    // комбинированная фильтрация
    [Fact]
    public void GetEvents_ShouldReturnsFilteredAndPaginatedResult()
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }

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

        var result = _eventService.GetEvents(searchQuery);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    // попытка получить событие с несуществующим ID
    [Fact]
    public void GetEvent_ShouldThrowNotFoundException()
    {
        var exception = () => _eventService.GetEvent(Guid.NewGuid());

        exception.Should().Throw<NotFoundException>();
    }

    // попытка обновить событие с несуществующим ID
    [Fact]
    public void UpdateEvent_ShouldThrowNotFoundException()
    {
        var newDto = new EventForUpdateQuery
        {
            Title = "Test",
            Description = "Test description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var exception = () => _eventService.UpdateEvent(Guid.NewGuid(), newDto);

        exception.Should().Throw<NotFoundException>();
    }

    // создание события с некорректными данными (если валидация в сервисе)
    [Fact]
    public void CreateEvent_EmptyTitle_ShouldThrowBadRequestException()
    {
        var newDto = new EventForCreationQuery
        {
            Title = null,
            Description = null,
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var exception = () => _eventService.CreateEvent(newDto);

        exception.Should().Throw<BadRequestException>();
    }

    // обновление события с некорректными датами (EndAt раньше StartAt)
    [Fact]
    public void UpdateEvent_EndAtBeforeStartAt_ShouldThrowBadRequestException()
    {
        foreach (var eventForCreation in _eventsForCreation)
        {
            _eventService.CreateEvent(eventForCreation);
        }
        
        var event1 = _eventService.GetEvents(new SearchEventsQuery()).Items.First();
        
        var newEvent = new EventForUpdateQuery()
        {
            Title = "Test",
            Description = "Test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(-1)
        };

        var exception = () => _eventService.UpdateEvent(event1.Id, newEvent);

        exception.Should().Throw<BadRequestException>();
    }
}