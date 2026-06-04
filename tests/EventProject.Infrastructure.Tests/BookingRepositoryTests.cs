using EventProject.Domain.Entities;
using EventProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Infrastructure.Tests;

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
            "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
    }

    [Fact]
    public async Task Add_SavesBookingToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var eventEntity = CreateEvent();
        await context.Events.AddAsync(eventEntity, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new BookingRepository(context);
        var booking = CreateBooking(eventEntity, BookingStatus.Pending);

        // Act
        await repository.Add(booking, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var saved = await verifyContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(eventEntity.Id, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
        Assert.Null(saved.ProcessedAt);
    }

    [Fact]
    public async Task GetById_ExistingBooking_ReturnsBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var eventEntity = CreateEvent();
        var booking = CreateBooking(eventEntity, BookingStatus.Confirmed, DateTime.UtcNow.AddMinutes(5));

        context.Events.Add(eventEntity);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new BookingRepository(context);

        // Act
        var result = await repository.GetById(booking.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(eventEntity.Id, result.EventId);
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

        var eventEntity = CreateEvent();
        var confirmedBooking = CreateBooking(eventEntity, BookingStatus.Confirmed, DateTime.UtcNow.AddMinutes(10));
        var pendingBooking = CreateBooking(eventEntity, BookingStatus.Pending);
        var rejectedBooking = CreateBooking(eventEntity, BookingStatus.Rejected, DateTime.UtcNow.AddMinutes(20));

        context.Events.Add(eventEntity);
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

    private static Event CreateEvent()
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 10,
            AvailableSeats = 10
        };
    }

    private static Booking CreateBooking(Event eventEntity, BookingStatus status, DateTime? processedAt = null)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventEntity.Id,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = processedAt,
            Event = eventEntity
        };
    }
}