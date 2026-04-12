using EventProject.Exceptions;
using EventProject.Models;
using EventProject.Repository.Booking;
using EventProject.Repository.Event;
using EventProject.Services.Booking;
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
            .Returns(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            });

        // Act
        var result = await _bookingService.CreateBookingAsync(eventId);

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
            .Returns(new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            });

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(eventId);
        var booking2 = await _bookingService.CreateBookingAsync(eventId);

        // Assert
        booking1.BookingId.Should().NotBe(booking2.BookingId);
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
            .Returns(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null
            });

        // Act
        var result = await _bookingService.GetBookingByIdAsync(bookingId);

        // Assert
        result.BookingId.Should().Be(bookingId);
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
            .Returns(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            })
            .Returns(new Booking
            {
                Id = bookingId,
                EventId = eventId,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.Now,
                ProcessedAt = DateTime.Now
            });

        // Act
        var first = await _bookingService.GetBookingByIdAsync(bookingId);
        var second = await _bookingService.GetBookingByIdAsync(bookingId);

        // Assert
        first.Status.Should().Be(BookingStatus.Pending);
        second.Status.Should().Be(BookingStatus.Confirmed);
        second.Status.Should().NotBe(first.Status);

        _bookingRepositoryMock.Verify(x => x.GetById(bookingId), Times.Exactly(2));
    }

    // Создание брони для несуществующего события
    // Создание брони для удалённого события
    [Fact]
    public async Task CreateBooking_ForNonExistentEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock
            .Setup(x => x.GetById(eventId))
            .Returns((Event?)null);

        // Act
        var action = () => _bookingService.CreateBookingAsync(eventId);

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
        var action = () => _bookingService.GetBookingByIdAsync(bookingId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}