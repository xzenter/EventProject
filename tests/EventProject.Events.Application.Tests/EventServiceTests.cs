using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Application.Abstractions.Services;
using EventProject.Events.Application.Caching;
using EventProject.Events.Application.Events;
using EventProject.Events.Application.Events.DTOs;
using EventProject.Events.Domain.Entities;
using EventProject.Events.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventProject.Events.Application.Tests
{
    public class EventServiceTest
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly CacheTtlOptions _cacheTtlOptions = new()
        {
            EventMinutes = 5,
            Top10EventsMinutes = 10
        };
        private readonly IOptions<CacheTtlOptions> _cacheOptions;

        public EventServiceTest()
        {
            _cacheOptions = Options.Create(_cacheTtlOptions);
        }

        private EventService CreateEventService()
        {
            return new EventService(_eventRepositoryMock.Object, _cacheServiceMock.Object, _cacheOptions);
        }

        private static Event CreateEventEntity(Guid? id = null)
        {
            return new Event
            {
                Id = id ?? Guid.NewGuid(),
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
                TotalSeats = 100,
                AvailableSeats = 100
            };
        }

        private static EventDto CreateEventDto(Guid? id = null)
        {
            var eventId = id ?? Guid.NewGuid();
            return new EventDto
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
                TotalSeats = 100,
                AvailableSeats = 100
            };
        }

        // ============================================================
        // GetEvent — cache hit scenarios
        // ============================================================

        [Fact]
        public async Task GetEvent_CacheHit_ReturnsCachedDataAndDoesNotCallRepository()
        {
            var eventId = Guid.NewGuid();
            var cachedDto = CreateEventDto(eventId);

            _cacheServiceMock
                .Setup(c => c.GetAsync<EventDto>(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedDto);

            var service = CreateEventService();
            var result = await service.GetEvent(eventId, CancellationToken.None);

            result.Should().BeEquivalentTo(cachedDto);
            _eventRepositoryMock.Verify(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<EventDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ============================================================
        // GetEvent — cache miss scenarios
        // ============================================================

        [Fact]
        public async Task GetEvent_CacheMiss_FetchesFromRepositoryAndSavesToCache()
        {
            var eventId = Guid.NewGuid();
            var eventEntity = CreateEventEntity(eventId);

            _cacheServiceMock
                .Setup(c => c.GetAsync<EventDto>(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto?)null);

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventEntity);

            var service = CreateEventService();
            var result = await service.GetEvent(eventId, CancellationToken.None);

            result.Id.Should().Be(eventId);
            result.Title.Should().Be(eventEntity.Title);

            _eventRepositoryMock.Verify(r => r.GetById(eventId, It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(
                c => c.SetAsync(
                    CacheKeys.Event(eventId),
                    It.Is<EventDto>(dto => dto.Id == eventId),
                    TimeSpan.FromMinutes(_cacheTtlOptions.EventMinutes),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEvent_CacheMissAndEventNotFound_ThrowsNotFoundException()
        {
            var eventId = Guid.NewGuid();

            _cacheServiceMock
                .Setup(c => c.GetAsync<EventDto>(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto?)null);

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();
            var act = () => service.GetEvent(eventId, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();

            _cacheServiceMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<EventDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ============================================================
        // GetTopEvents — cache hit scenarios
        // ============================================================

        [Fact]
        public async Task GetTopEvents_CacheHit_ReturnsCachedDataAndDoesNotCallRepository()
        {
            var cachedEvents = new List<EventDto> { CreateEventDto(), CreateEventDto() };

            _cacheServiceMock
                .Setup(c => c.GetAsync<IReadOnlyCollection<EventDto>>(CacheKeys.Top10Events(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedEvents);

            var service = CreateEventService();
            var result = await service.GetTopEvents(CancellationToken.None);

            result.Should().BeEquivalentTo(cachedEvents);
            _eventRepositoryMock.Verify(r => r.GetTopEvents(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ============================================================
        // GetTopEvents — cache miss scenarios
        // ============================================================

        [Fact]
        public async Task GetTopEvents_CacheMiss_FetchesFromRepositoryAndSavesToCache()
        {
            var events = new List<Event> { CreateEventEntity(), CreateEventEntity() };

            _cacheServiceMock
                .Setup(c => c.GetAsync<IReadOnlyCollection<EventDto>>(CacheKeys.Top10Events(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyCollection<EventDto>?)null);

            _eventRepositoryMock
                .Setup(r => r.GetTopEvents(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            var service = CreateEventService();
            var result = await service.GetTopEvents(CancellationToken.None);

            result.Should().HaveCount(2);
            _eventRepositoryMock.Verify(r => r.GetTopEvents(10, It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(
                c => c.SetAsync(
                    CacheKeys.Top10Events(),
                    It.IsAny<IReadOnlyCollection<EventDto>>(),
                    TimeSpan.FromMinutes(_cacheTtlOptions.Top10EventsMinutes),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ============================================================
        // CreateEvent — mutating operation
        // ============================================================

        [Fact]
        public async Task CreateEvent_WithValidData_AddsToRepositoryAndReturnsDto()
        {
            var query = new EventForCreationQuery
            {
                Title = "New Event",
                Description = "New Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(3),
                TotalSeats = 50
            };

            _eventRepositoryMock
                .Setup(r => r.Add(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _eventRepositoryMock
                .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var service = CreateEventService();
            var result = await service.CreateEvent(query, CancellationToken.None);

            result.Title.Should().Be(query.Title);
            result.Description.Should().Be(query.Description);
            result.StartAt.Should().Be(query.StartAt);
            result.EndAt.Should().Be(query.EndAt);
            result.TotalSeats.Should().Be(query.TotalSeats);
            result.AvailableSeats.Should().Be(query.TotalSeats);

            _eventRepositoryMock.Verify(r => r.Add(It.Is<Event>(e =>
                e.Title == query.Title &&
                e.TotalSeats == query.TotalSeats &&
                e.AvailableSeats == query.TotalSeats
            ), It.IsAny<CancellationToken>()), Times.Once);

            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_WithEmptyTitle_ThrowsBadRequestException()
        {
            var query = new EventForCreationQuery
            {
                Title = "",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(3),
                TotalSeats = 50
            };

            var service = CreateEventService();
            var act = () => service.CreateEvent(query, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>();

            _eventRepositoryMock.Verify(r => r.Add(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateEvent_WithStartDateAfterEndDate_ThrowsBadRequestException()
        {
            var query = new EventForCreationQuery
            {
                Title = "Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(2),
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 50
            };

            var service = CreateEventService();
            var act = () => service.CreateEvent(query, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>();

            _eventRepositoryMock.Verify(r => r.Add(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ============================================================
        // UpdateEvent — mutating operation: updates cache
        // ============================================================

        [Fact]
        public async Task UpdateEvent_WithValidData_UpdatesRepositoryAndUpdatesCache()
        {
            var eventId = Guid.NewGuid();
            var existingEvent = CreateEventEntity(eventId);

            var query = new EventForUpdateQuery
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartAt = DateTime.UtcNow.AddDays(5),
                EndAt = DateTime.UtcNow.AddDays(5).AddHours(4)
            };

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEvent);

            _eventRepositoryMock
                .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var service = CreateEventService();
            var result = await service.UpdateEvent(eventId, query, CancellationToken.None);

            result.Id.Should().Be(eventId);
            result.Title.Should().Be(query.Title);
            result.Description.Should().Be(query.Description);
            result.StartAt.Should().Be(query.StartAt);
            result.EndAt.Should().Be(query.EndAt);

            _eventRepositoryMock.Verify(r => r.GetById(eventId, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);

            _cacheServiceMock.Verify(
                c => c.SetAsync(
                    CacheKeys.Event(eventId),
                    It.Is<EventDto>(dto => dto.Id == eventId && dto.Title == query.Title),
                    TimeSpan.FromMinutes(_cacheTtlOptions.EventMinutes),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEvent_WithStartDateAfterEndDate_ThrowsBadRequestException()
        {
            var query = new EventForUpdateQuery
            {
                Title = "Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(2),
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            var service = CreateEventService();
            var act = () => service.UpdateEvent(Guid.NewGuid(), query, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>();

            _eventRepositoryMock.Verify(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateEvent_WhenEventNotFound_ThrowsNotFoundException()
        {
            var eventId = Guid.NewGuid();
            var query = new EventForUpdateQuery
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();
            var act = () => service.UpdateEvent(eventId, query, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();

            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
            _cacheServiceMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<EventDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ============================================================
        // DeleteEvent — mutating operation: removes from cache
        // ============================================================

        [Fact]
        public async Task DeleteEvent_WhenEventExists_RemovesFromRepositoryAndInvalidatesCache()
        {
            var eventId = Guid.NewGuid();
            var existingEvent = CreateEventEntity(eventId);

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEvent);

            _eventRepositoryMock
                .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var service = CreateEventService();
            await service.DeleteEvent(eventId, CancellationToken.None);

            _eventRepositoryMock.Verify(r => r.GetById(eventId, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(r => r.Delete(existingEvent, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);

            _cacheServiceMock.Verify(
                c => c.RemoveAsync(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_WhenEventNotFound_ThrowsNotFoundException()
        {
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetById(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();
            var act = () => service.DeleteEvent(eventId, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();

            _eventRepositoryMock.Verify(r => r.Delete(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
            _eventRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
            _cacheServiceMock.Verify(
                c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}