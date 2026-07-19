using EventProject.Bookings.Domain.Entities;
using EventProject.Bookings.Domain.Enums;
using EventProject.Bookings.Infrastructure.Repositories;
using EventProject.Bookings.Infrastructure.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Bookings.Infrastructure.Tests;

[Collection("Database collection")]
public class BookingRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public BookingRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = _fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE bookings RESTART IDENTITY CASCADE");
    }

    [Fact]
    public async Task Add_SavesBookingToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var bookingRepository = new BookingRepository(context);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await bookingRepository.Add(booking, CancellationToken.None);
        await bookingRepository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var saved = await verifyContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
        Assert.Null(saved.ProcessedAt);
    }

    [Fact]
    public async Task GetById_ExistingBooking_ReturnsBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new BookingRepository(context);

        // Act
        var result = await repository.GetById(booking.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task GetById_UnknownBooking_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();
        var repository = new BookingRepository(context);

        // Act
        var result = await repository.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByStatus_ReturnsOnlyBookingsWithRequestedStatus()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();


        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var confirmedBooking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow.AddMinutes(10)
        };

        var pendingBooking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        var rejectedBooking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Rejected,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow.AddMinutes(20)
        };

        context.Bookings.AddRange(confirmedBooking, pendingBooking, rejectedBooking);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new BookingRepository(context);

        // Act
        var result = (await repository.GetByStatus(BookingStatus.Confirmed, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(confirmedBooking.Id, result[0].Id);
        Assert.Equal(BookingStatus.Confirmed, result[0].Status);
    }
}