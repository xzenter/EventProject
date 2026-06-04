using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Event;
using EventProject.Application.Event.DTOs;
using EventProject.Domain.Entities;
using EventProject.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace EventProject.Application.Tests;

public class EventServiceTests
{
    private readonly EventService _eventService;
    private readonly Mock<IEventRepository> _eventRepositoryMock;

    public EventServiceTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _eventService = new EventService(_eventRepositoryMock.Object);
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
    public async Task CreateEvent_ShouldReturnsEvent()
    {
        var eventForCreation = new EventForCreationQuery
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var eventDto = await _eventService.CreateEvent(eventForCreation, CancellationToken.None);

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
        // Arrange
        var events = _eventsForCreation.Select((e, _) => new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        }).ToList();

        _eventRepositoryMock
            .Setup(x => x.GetByFilter(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var searchQuery = new SearchEventsQuery
        {
            Title = null,
            From = null,
            To = null,
            Page = 1,
            PageSize = _eventsForCreation.Count
        };

        var result = await _eventService.GetEvents(searchQuery, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(_eventsForCreation.Count);
        result.TotalItems.Should().Be(_eventsForCreation.Count);
    }

    // получение события по ID
    [Fact]
    public async Task GetEventById_ShouldReturnsEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new Domain.Entities.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 1,
            AvailableSeats = 1
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _eventService.GetEvent(eventId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(eventId);
        result.Title.Should().Be(eventEntity.Title);
        result.Description.Should().Be(eventEntity.Description);
        result.StartAt.Should().Be(eventEntity.StartAt);
        result.EndAt.Should().Be(eventEntity.EndAt);
    }

    // обновление существующего события
    [Fact]
    public async Task UpdateEvent_ShouldReturnsUpdatedEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventForCreation = _eventsForCreation[0];
    
        var eventEntity = new Domain.Entities.Event
        {
            Id = eventId,
            Title = eventForCreation.Title,
            Description = eventForCreation.Description,
            StartAt = eventForCreation.StartAt,
            EndAt = eventForCreation.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock
            .Setup(x => x.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var eventForUpdate = new EventForUpdateQuery
        {
            Title = "New Title",
            Description = "New Description",
            StartAt = DateTime.Now.AddDays(10),
            EndAt = DateTime.Now.AddDays(20)
        };

        // Act
        var result = await _eventService.UpdateEvent(eventId, eventForUpdate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(eventId);
        result.Title.Should().Be(eventForUpdate.Title);
        result.Description.Should().Be(eventForUpdate.Description);
        result.StartAt.Should().Be(eventForUpdate.StartAt);
        result.EndAt.Should().Be(eventForUpdate.EndAt);
    }

    // удаление существующего события
    [Fact]
    public async Task DeleteEvent_ShouldDeleteEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var eventEntity = new Domain.Entities.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 1,
            AvailableSeats = 1
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock
            .Setup(x => x.Delete(eventEntity))
            .Verifiable();

        // Act
        await _eventService.DeleteEvent(eventId, CancellationToken.None);

        // Assert
        _eventRepositoryMock.Verify(x => x.GetById(eventId), Times.Once);
        _eventRepositoryMock.Verify(x => x.Delete(eventEntity), Times.Once);
    }

    // фильтрация по названию
    [Fact]
    public async Task GetEvents_ShouldReturnsFilteredEvents()
    {
        // Arrange
        var events = _eventsForCreation.Select((e, _) => new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        }).ToList();

        _eventRepositoryMock
            .Setup(x => x.GetByFilter("Test Event 2", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events.Where(e => e.Title == "Test Event 2").ToList());

        // Act
        var searchQuery = new SearchEventsQuery
        {
            Title = "Test Event 2",
            From = null,
            To = null,
            Page = 1,
            PageSize = 10
        };

        var result = await _eventService.GetEvents(searchQuery, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        result.Items.First().Title.Should().Be(searchQuery.Title);
    }

    // фильтрация по датам (startDate, endDate)
    [Fact]
    public async Task GetEvents_ShouldReturnsFilteredEventsByDates()
    {
        // Arrange
        var events = _eventsForCreation.Select((e, _) => new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        }).ToList();

        var event1 = _eventsForCreation.First();

        _eventRepositoryMock
            .Setup(x => x.GetByFilter(null, event1.StartAt, event1.EndAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events.Take(1).ToList());

        // Act
        var searchQuery = new SearchEventsQuery
        {
            // Title = null,
            From = event1.StartAt,
            To = event1.EndAt,
            Page = 1,
            PageSize = 10
        };

        var result = await _eventService.GetEvents(searchQuery, CancellationToken.None);

        // Assert
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
        // Arrange
        var events = _eventsForCreation.Select((e, _) => new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        }).ToList();

        _eventRepositoryMock
            .Setup(x => x.GetByFilter(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var searchQuery = new SearchEventsQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await _eventService.GetEvents(searchQuery, CancellationToken.None);

        // Assert
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
        // Arrange
        var events = _eventsForCreation.Select((e, _) => new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        }).ToList();

        var element1 = _eventsForCreation.First();
        var element2 = _eventsForCreation.Skip(1).First();

        _eventRepositoryMock
            .Setup(x => x.GetByFilter("Test", element1.StartAt, element2.EndAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events.Where(e => e.Title.Contains("Test") && e.StartAt >= element1.StartAt && e.EndAt <= element2.EndAt).ToList());

        // Act
        var searchQuery = new SearchEventsQuery
        {
            Title = "Test",
            From = element1.StartAt,
            To = element2.EndAt,
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await _eventService.GetEvents(searchQuery, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    // попытка получить событие с несуществующим ID
    [Fact]
    public async Task GetEvent_ShouldThrowNotFoundException()
    {
        var exception = () => _eventService.GetEvent(Guid.NewGuid());

        await exception.Should().ThrowAsync<NotFoundException>();
    }

    // попытка обновить событие с несуществующим ID
    [Fact]
    public async Task UpdateEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var newDto = new EventForUpdateQuery
        {
            Title = "Test",
            Description = "Test description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync((Domain.Entities.Event?)null);

        // Act
        var exception = () => _eventService.UpdateEvent(eventId, newDto);

        // Assert
        await exception.Should().ThrowAsync<NotFoundException>();
    }

    // создание события с некорректными данными (если валидация в сервисе)
    [Fact]
    public async Task CreateEvent_EmptyTitle_ShouldThrowBadRequestException()
    {
        var newDto = new EventForCreationQuery
        {
            Title = null!,
            Description = null,
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1)
        };

        var exception = () => _eventService.CreateEvent(newDto);

        await exception.Should().ThrowAsync<BadRequestException>();
    }

    // обновление события с некорректными датами (EndAt раньше StartAt)
    [Fact]
    public async Task UpdateEvent_EndAtBeforeStartAt_ShouldThrowBadRequestException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventForCreation = _eventsForCreation[0];
    
        var eventEntity = new Domain.Entities.Event
        {
            Id = eventId,
            Title = eventForCreation.Title,
            Description = eventForCreation.Description,
            StartAt = eventForCreation.StartAt,
            EndAt = eventForCreation.EndAt,
            TotalSeats = 10,
            AvailableSeats = 10
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(eventEntity);

        var newEvent = new EventForUpdateQuery()
        {
            Title = "Test",
            Description = "Test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(-1)
        };

        // Act
        var exception = () => _eventService.UpdateEvent(eventId, newEvent);

        // Assert
        await exception.Should().ThrowAsync<BadRequestException>();
    }
}