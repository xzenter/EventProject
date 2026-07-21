using EventProject.Bookings.Application.Abstractions.Repositories;
using EventProject.Bookings.Application.Booking;
using EventProject.Bookings.Domain.Enums;
using EventProject.Bookings.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventProject.Bookings.Application.Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly BookingSettings _bookingSettings;

    public BookingServiceTests()
    {
        _bookingSettings = new BookingSettings
        {
            MaxActiveBookings = 10
        };

        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _bookingService = new BookingService(
            _bookingRepositoryMock.Object,
            Options.Create(_bookingSettings)
        );
    }

    private static Domain.Entities.Booking CreatePendingBooking(Guid? bookingId = null, Guid? eventId = null, Guid? userId = null)
    {
        return new Domain.Entities.Booking
        {
            Id = bookingId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null
        };
    }

    // ============================================================
    // CreateBooking
    // ============================================================

    [Fact]
    public async Task CreateBooking_UnderLimit_CreatesAndReturnsBookingInfo()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetActiveBookingsCount(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _bookingRepositoryMock
            .Setup(r => r.Add(It.IsAny<Domain.Entities.Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _bookingRepositoryMock
            .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        result.EventId.Should().Be(eventId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(BookingStatus.Pending);
        result.ProcessedAt.Should().BeNull();

        _bookingRepositoryMock.Verify(r => r.GetActiveBookingsCount(userId, It.IsAny<CancellationToken>()), Times.Once);
        _bookingRepositoryMock.Verify(r => r.Add(It.Is<Domain.Entities.Booking>(b =>
            b.EventId == eventId &&
            b.UserId == userId &&
            b.Status == BookingStatus.Pending
        ), It.IsAny<CancellationToken>()), Times.Once);
        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ExceedsLimit_ThrowsActiveBookingLimitExceededException()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetActiveBookingsCount(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_bookingSettings.MaxActiveBookings);

        var act = () => _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        await act.Should().ThrowAsync<ActiveBookingLimitExceededException>();

        _bookingRepositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBooking_AtExactLimit_ThrowsActiveBookingLimitExceededException()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetActiveBookingsCount(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_bookingSettings.MaxActiveBookings);

        var act = () => _bookingService.CreateBooking(eventId, userId, CancellationToken.None);

        await act.Should().ThrowAsync<ActiveBookingLimitExceededException>();

        _bookingRepositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ============================================================
    // GetBookingById
    // ============================================================

    [Fact]
    public async Task GetBookingById_WhenExists_ReturnsBookingInfo()
    {
        var bookingId = Guid.NewGuid();
        var booking = CreatePendingBooking(bookingId);

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _bookingService.GetBookingById(bookingId, CancellationToken.None);

        result.BookingId.Should().Be(booking.Id);
        result.EventId.Should().Be(booking.EventId);
        result.UserId.Should().Be(booking.UserId);
        result.Status.Should().Be(booking.Status);
        result.CreatedAt.Should().Be(booking.CreatedAt);
        result.ProcessedAt.Should().Be(booking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingById_WhenNotExists_ThrowsNotFoundException()
    {
        var bookingId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Booking?)null);

        var act = () => _bookingService.GetBookingById(bookingId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // CancelBooking
    // ============================================================

    [Fact]
    public async Task CancelBooking_ByAdmin_CancelsSuccessfully()
    {
        var bookingId = Guid.NewGuid();
        var booking = CreatePendingBooking(bookingId);

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _bookingRepositoryMock
            .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _bookingService.CancelBooking(bookingId, Guid.NewGuid(), Role.Admin, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.ProcessedAt.Should().NotBeNull();

        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelBooking_ByOwner_CancelsSuccessfully()
    {
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = CreatePendingBooking(bookingId, userId: userId);

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _bookingRepositoryMock
            .Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _bookingService.CancelBooking(bookingId, userId, Role.User, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelBooking_ByOtherUser_ThrowsBookingAccessDeniedException()
    {
        var bookingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var booking = CreatePendingBooking(bookingId, userId: ownerId);

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var act = () => _bookingService.CancelBooking(bookingId, otherUserId, Role.User, CancellationToken.None);

        await act.Should().ThrowAsync<BookingAccessDeniedException>();

        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_WhenNotExists_ThrowsNotFoundException()
    {
        var bookingId = Guid.NewGuid();

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Booking?)null);

        var act = () => _bookingService.CancelBooking(bookingId, Guid.NewGuid(), Role.User, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_WhenAlreadyCancelled_ThrowsBadRequestException()
    {
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = CreatePendingBooking(bookingId, userId: userId);
        booking.Cancel(DateTime.UtcNow);

        _bookingRepositoryMock
            .Setup(r => r.GetById(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var act = () => _bookingService.CancelBooking(bookingId, userId, Role.User, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();

        _bookingRepositoryMock.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }
}