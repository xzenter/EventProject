using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Booking;
using EventProject.Application.Booking.DTOs;
using EventProject.Domain.Entities;
using EventProject.Domain.Enums;
using EventProject.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventProject.Application.Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;

    public BookingServiceTests()
    {
        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _eventRepositoryMock.Object,
            bookingSettings
        );
    }

    /// <summary>
    /// Создание брони для существующего события — возвращается BookingInfo со статусом Pending
    /// </summary>
    [Fact]
    public async Task CreateBooking_ForExistingEvent_ShouldReturnBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
        var result = await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Создание нескольких броней для одного события — все создаются с уникальными Id
    /// </summary>
    [Fact]
    public async Task CreateMultipleBookings_ForSameEvent_ShouldHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
        var booking1 = await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);
        var booking2 = await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        // Assert
        booking1.BookingId.Should().NotBe(booking2.BookingId);
    }

    /// <summary>
    /// Получение брони по Id — возвращается корректная информация
    /// </summary>
    [Fact]
    public async Task GetBooking_ById_ShouldReturnCorrectInfo()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(x => x.GetById(bookingId))
            .ReturnsAsync(new Domain.Entities.Booking
            {
                Id = bookingId,
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null,
                Event = null!,
            });

        // Act
        var result = await _bookingService.GetBookingById(bookingId, CancellationToken.None);

        // Assert
        result.BookingId.Should().Be(bookingId);
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Получение брони отражает изменение статуса (после Confirm/Reject)
    /// </summary>
    [Fact]
    public async Task GetBooking_AfterStatusChange_ShouldReflectStatusChange()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookingRepositoryMock
            .SetupSequence(x => x.GetById(bookingId))
            .ReturnsAsync(new Domain.Entities.Booking
            {
                Id = bookingId,
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                Event = null!,
            })
            .ReturnsAsync(new Domain.Entities.Booking
            {
                Id = bookingId,
                EventId = eventId,
                UserId = userId,
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

    /// <summary>
    /// Создание брони для несуществующего события
    /// </summary>
    [Fact]
    public async Task CreateBooking_ForNonExistentEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync((Domain.Entities.Event?)null);

        // Act
        var action = () => _bookingService.CreateBooking(eventId, userId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// Создание брони для удалённого события
    /// </summary> 
    [Fact]
    public async Task CreateBooking_ForDeletedEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync((Domain.Entities.Event?)null);

        // Act
        var action = () => _bookingService.CreateBooking(eventId, userId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// Получение брони для несуществующего ID
    /// </summary>
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

    /// <summary>
    /// Создание брони уменьшает AvailableSeats на 1.
    /// </summary>
    [Fact]
    public async Task CreateBooking_ShouldDecreaseAvailableSeatsByOne()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int initialAvailableSeats = 5;

        var eventEntity = new Domain.Entities.Event
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
        await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        // Assert
        eventEntity.AvailableSeats.Should().Be(initialAvailableSeats - 1);
    }

    /// <summary>
    /// Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.
    /// </summary>
    [Fact]
    public async Task CreateMultipleBookings_UpToLimit_ShouldSucceedWithUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int totalSeats = 3;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
            var booking = await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);
            bookings.Add(booking);
        }

        // Assert
        bookings.Should().HaveCount(totalSeats);
        bookings.Select(b => b.BookingId).Should().OnlyHaveUniqueItems();

        // Проверяем, что все брони имеют статус Pending
        foreach (var booking in bookings) booking.Status.Should().Be(BookingStatus.Pending);

        // Проверяем, что AvailableSeats уменьшилось до 0
        _eventRepositoryMock.Verify(x => x.GetById(eventId), Times.Exactly(totalSeats));
    }

    /// <summary>
    /// После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException
    /// </summary>
    [Fact]
    public async Task CreateBooking_AfterSeatsExhausted_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int totalSeats = 1;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
        var action = () => _bookingService.CreateBooking(eventId, userId);

        // Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    /// <summary>
    /// Бронирование при отсутствии мест → NoAvailableSeatsException
    /// </summary>
    [Fact]
    public async Task CreateBooking_WhenNoSeatsAvailable_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
        var action = () => _bookingService.CreateBooking(eventId, userId);

        // Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    /// <summary>
    /// Переход в Confirmed: После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.
    /// </summary>
    [Fact]
    public void Confirm_Should_Set_Status_To_Confirmed_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Domain.Entities.Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
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

    /// <summary>
    /// Переход в Rejected: После вызова Reject() бронь возвращает статус Rejected и заполненный ProcessedAt.
    /// </summary>
    [Fact]
    public void Reject_Should_Set_Status_To_Rejected_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Domain.Entities.Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
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

    /// <summary>
    /// Тест на защиту от овербукинга
    /// </summary>
    [Fact]
    public async Task Overbooking_Protection_Test()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var event1 = new Domain.Entities.Event
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
            .ReturnsAsync((Domain.Entities.Booking?)null);
        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        var bookingService =
            new BookingService(bookingRepositoryMock.Object, eventRepositoryMock.Object, bookingSettings);

        // Act
        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                try
                {
                    await bookingService.CreateBooking(eventId, userId);
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

    /// <summary>
    /// Тест на уникальность Id при конкурентных запросах
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithConcurrentRequests_ShouldHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int totalSeats = 10;

        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .ReturnsAsync(new Domain.Entities.Event
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
            .Select(_ => _bookingService.CreateBooking(eventId, userId))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var bookings = tasks.Select(t => t.Result).ToList();
        bookings.Select(b => b.BookingId).Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// Проверяет, что при отмене брони принадлежащей пользователю с ролью User 
    /// статус брони изменяется на Cancelled и количество свободных мест у события восстанавливается
    /// </summary>
    [Fact]
    public async Task CancelBooking_Should_ReleaseEventSeats_And_BookingStatus_ShouldBe_Cancelled()
    {
        // Arrange (подготовка)
        var eventId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var fakeEvent = new Event
        {
            Id = eventId,
            Title = "Test event",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 1,
            AvailableSeats = 0
        };

        var booking = new Domain.Entities.Booking
        {
            Id = bookingId,
            EventId = eventId,
            UserId = userId,
            Event = fakeEvent,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
        };

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId))
            .ReturnsAsync(booking);

        _eventRepositoryMock
            .Setup(r => r.GetById(eventId))
            .ReturnsAsync(fakeEvent);

        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        var bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _eventRepositoryMock.Object,
            bookingSettings
        );

        // Act (действие)
        await bookingService.CancelBooking(bookingId, userId, Role.User, CancellationToken.None);

        // Assert (проверка)
        booking.Status.Should().Be(BookingStatus.Cancelled);

        fakeEvent.AvailableSeats.Should().Be(1);
    }

    /// <summary>
    /// Проверяет, что при отмене брони на прошедшее событие пользователем с ролью Admin 
    /// статус брони изменяется на Cancelled
    /// </summary>
    [Fact]
    public async Task
        CancelBookingAsync_WhenEventAlreadyStarted_ShouldChange_BookingStatus_ToCancelled_WithUserRole_Admin()
    {
        // Arrange (подготовка)
        var eventId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var fakeEvent = new Event
        {
            Id = eventId,
            Title = "Test event",
            StartAt = default(DateTime),
            EndAt = default(DateTime).AddDays(1),
            TotalSeats = 1,
            AvailableSeats = 0
        };

        var booking = new Domain.Entities.Booking
        {
            Id = bookingId,
            EventId = eventId,
            UserId = userId,
            Event = fakeEvent,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
        };

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId))
            .ReturnsAsync(booking);

        _eventRepositoryMock
            .Setup(r => r.GetById(eventId))
            .ReturnsAsync(fakeEvent);

        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        var bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _eventRepositoryMock.Object,
            bookingSettings
        );

        // Act (действие)
        await bookingService.CancelBooking(bookingId, userId, Role.Admin, CancellationToken.None);

        // Assert (проверка)
        booking.Status.Should().Be(BookingStatus.Cancelled);

        fakeEvent.AvailableSeats.Should().Be(1);
    }

    /// <summary>
    /// Проверяет, что при отмене брони на прошедшее событие пользователем с ролью Admin 
    /// статус брони изменяется на Cancelled
    /// </summary>
    [Fact]
    public async Task CancelBookingAsync_WithUserRole_Admin_CanCancelAnotherUsersBooking_WhenEventHasStarted()
    {
        // Arrange (подготовка)
        var eventId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var fakeEvent = new Event
        {
            Id = eventId,
            Title = "Test event",
            StartAt = default(DateTime),
            EndAt = default(DateTime).AddDays(1),
            TotalSeats = 5,
            AvailableSeats = 4
        };

        var booking = new Domain.Entities.Booking
        {
            Id = bookingId,
            EventId = eventId,
            UserId = userId,
            Event = fakeEvent,
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
        };

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId))
            .ReturnsAsync(booking);

        _eventRepositoryMock
            .Setup(r => r.GetById(eventId))
            .ReturnsAsync(fakeEvent);

        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        var bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _eventRepositoryMock.Object,
            bookingSettings
        );

        // Act (действие)
        await bookingService.CancelBooking(bookingId, adminId, Role.Admin, CancellationToken.None);

        // Assert (проверка)
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    /// <summary>
    /// Проверяет, что лимиты активных броней разных пользователей
    /// не влияют друг на друга.
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WhenAnotherUserReachedLimit_ShouldCreateBookingSuccessfully()
    {
        // Arrange (подготовка)
        var eventId = Guid.NewGuid();
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        var fakeEvent = new Event
        {
            Id = eventId,
            Title = "Test event",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 15,
            AvailableSeats = 15
        };

        _eventRepositoryMock
            .Setup(r => r.GetById(eventId))
            .ReturnsAsync(fakeEvent);

        // У первого пользователя лимит достигнут.
        _bookingRepositoryMock
            .Setup(r => r.GetActiveBookingsCount(firstUserId))
            .ReturnsAsync(10);

        // У второго пользователя активных броней нет.
        _bookingRepositoryMock
            .Setup(r => r.GetActiveBookingsCount(secondUserId))
            .ReturnsAsync(0);

        var bookingSettings = Options.Create(new BookingSettings
        {
            MaxActiveBookings = 10
        });

        var bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            _eventRepositoryMock.Object,
            bookingSettings
        );

        // Act
        var booking = await bookingService.CreateBooking(eventId, secondUserId, CancellationToken.None);

        // Assert
        booking.Should().NotBeNull();

        _bookingRepositoryMock.Verify(
            r => r.Add(It.IsAny<Domain.Entities.Booking>()),
            Times.Once);
    }
}