using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Booking;
using EventProject.Domain.Entities;
using EventProject.Domain.Exceptions;
using EventProject.Presentation.Repository.Booking;
using EventProject.Presentation.Repository.Event;
using FluentAssertions;
using Moq;

namespace EventProject.Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;

    public BookingServiceTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _bookingService = new BookingService(_bookingRepositoryMock.Object, _eventRepositoryMock.Object);
    }

    // Создание брони для существующего события — возвращается BookingInfo со статусом Pending
    [Fact]
    public async Task CreateBooking_ForExistingEvent_ShouldReturnBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1
            });

        // Act
        var result = await _bookingService.CreateBooking(eventId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    // Создание нескольких броней для одного события — все создаются с уникальными Id
    [Fact]
    public async Task CreateMultipleBookings_ForSameEvent_ShouldHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = 2,
                AvailableSeats = 2
            });

        // Act
        var booking1 = await _bookingService.CreateBooking(eventId, CancellationToken.None);
        var booking2 = await _bookingService.CreateBooking(eventId, CancellationToken.None);

        // Assert
        booking1.Id.Should().NotBe(booking2.Id);
    }

    // Получение брони по Id — возвращается корректная информация
    [Fact]
    public async Task GetBooking_ById_ShouldReturnCorrectInfo()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(x => x.GetById(bookingId))
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null,
                Event = null!,
            });

        // Act
        var result = await _bookingService.GetBookingById(bookingId, CancellationToken.None);

        // Assert
        result.Id.Should().Be(bookingId);
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    // Получение брони отражает изменение статуса (после Confirm/Reject)
    [Fact]
    public async Task GetBooking_AfterStatusChange_ShouldReflectStatusChange()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _bookingRepositoryMock
            .SetupSequence(x => x.GetById(bookingId))
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                Event = null!,
            })
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.Now,
                ProcessedAt = DateTime.Now,
                Event = null!,
            });

        // Act
        var first = await _bookingService.GetBookingById(bookingId, CancellationToken.None);
        var second = await _bookingService.GetBookingById(bookingId, CancellationToken.None);

        // Assert
        first.Status.Should().Be(BookingStatus.Pending);
        second.Status.Should().Be(BookingStatus.Confirmed);
        second.Status.Should().NotBe(first.Status);

        _bookingRepositoryMock.Verify(x => x.GetById(bookingId), Times.Exactly(2));
    }

    // Создание брони для несуществующего события
    [Fact]
    public async Task CreateBooking_ForNonExistentEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var action = () => _bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Создание брони для удалённого события
    [Fact]
    public async Task CreateBooking_ForDeletedEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var action = () => _bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Получение брони по несуществующему Id
    [Fact]
    public async Task GetBooking_ForNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var action = () => _bookingService.GetBookingById(bookingId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Создание брони уменьшает AvailableSeats на 1.
    [Fact]
    public async Task CreateBooking_ShouldDecreaseAvailableSeatsByOne()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int initialAvailableSeats = 5;
        var eventEntity = new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = initialAvailableSeats,
            AvailableSeats = initialAvailableSeats
        };

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(eventEntity);

        // Act
        await _bookingService.CreateBooking(eventId, CancellationToken.None);

        // Assert
        eventEntity.AvailableSeats.Should().Be(initialAvailableSeats - 1);
    }

    // Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.
    [Fact]
    public async Task CreateMultipleBookings_UpToLimit_ShouldSucceedWithUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int totalSeats = 3;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats
            });

        // Act
        var bookings = new List<BookingInfo>();
        for (var i = 0; i < totalSeats; i++)
        {
            var booking = await _bookingService.CreateBooking(eventId, CancellationToken.None);
            bookings.Add(booking);
        }

        // Assert
        bookings.Should().HaveCount(totalSeats);
        bookings.Select(b => b.Id).Should().OnlyHaveUniqueItems();

        // Проверяем, что все брони имеют статус Pending
        foreach (var booking in bookings) booking.Status.Should().Be(BookingStatus.Pending);

        // Проверяем, что AvailableSeats уменьшилось до 0
        _eventRepositoryMock.Verify(x => x.GetById(eventId), Times.Exactly(totalSeats));
    }

    // После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException
    [Fact]
    public async Task CreateBooking_AfterSeatsExhausted_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int totalSeats = 1;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = totalSeats,
                AvailableSeats = 0 // Все места уже заняты
            });

        // Act
        var action = () => _bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    // Бронирование при отсутствии мест → NoAvailableSeatsException
    [Fact]
    public async Task CreateBooking_WhenNoSeatsAvailable_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = 0,
                AvailableSeats = 0
            });

        // Act
        var action = () => _bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    // Переход в Confirmed: После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.
    [Fact]
    public void Confirm_Should_Set_Status_To_Confirmed_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            ProcessedAt = null,
            Event = null!,
        };

        // Act
        booking.Confirm(DateTime.UtcNow);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    // Переход в Rejected: После вызова Reject() бронь возвращает статус Rejected и заполненный ProcessedAt.
    [Fact]
    public void Reject_Should_Set_Status_To_Rejected_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            ProcessedAt = null,
            Event = null!,
        };

        // Act
        booking.Reject(DateTime.UtcNow);

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    // Тест на защиту от овербукинга
    [Fact]
    public async Task Overbooking_Protection_Test()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var event1 = new Event
        {
            Id = eventId,
            Title = "TestEvent",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 5,
            AvailableSeats = 5
        };

        var eventRepositoryMock = new Mock<IEventRepository>();
        var bookingRepositoryMock = new Mock<IBookingRepository>();

        eventRepositoryMock
            .Setup(e => e.GetById(eventId))
            .ReturnsAsync(event1);
        bookingRepositoryMock
            .Setup(b => b.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Booking?)null);

        var bookingService = new BookingService(bookingRepositoryMock.Object, eventRepositoryMock.Object);

        // Act
        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                try
                {
                    await bookingService.CreateBooking(eventId);
                    return (Success: true, Exception: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Success: false, Exception: ex);
                }
            });

        var results = await Task.WhenAll(tasks);

        // Assert
        // Проверяем, что только 5 запросов были успешными, а остальные 15 вызвали NoAvailableSeatsException
        results.Count(r => r.Success).Should().Be(5);
        results.Count(r => r.Exception is NoAvailableSeatsException).Should().Be(15);

        // Проверяем, что количество доступных мест стало 0
        event1.AvailableSeats.Should().Be(0);
    }

    // Тест на уникальность Id при конкурентных запросах
    [Fact]
    public async Task CreateBooking_WithConcurrentRequests_ShouldHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int totalSeats = 10;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats
            });

        // Act
        var tasks = Enumerable.Range(0, totalSeats)
            .Select(_ => _bookingService.CreateBooking(eventId))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var bookings = tasks.Select(t => t.Result).ToList();
        bookings.Select(b => b.Id).Should().OnlyHaveUniqueItems();
    }
}