using EventProject.Domain.Entities;
using EventProject.Infrastructure.Repositories;
using EventProject.Infrastructure.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Infrastructure.Tests;

[Collection("Database collection")]
public class EventRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public EventRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = _fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE bookings, events, users RESTART IDENTITY CASCADE");
    }

    [Fact]
    public async Task Add_SavesEventToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var repository = new EventRepository(context);
        var eventEntity = CreateEvent("Repository conference", DateTime.UtcNow.AddDays(1));

        // Act
        await repository.Add(eventEntity, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var saved = await verifyContext.Events
            .FirstOrDefaultAsync(e => e.Id == eventEntity.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal("Repository conference", saved.Title);
        Assert.Equal(eventEntity.StartAt, saved.StartAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(eventEntity.EndAt, saved.EndAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(100, saved.TotalSeats);
        Assert.Equal(100, saved.AvailableSeats);
    }

    [Fact]
    public async Task GetById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var eventEntity = CreateEvent("Existing event", DateTime.UtcNow.AddDays(3));
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetById(eventEntity.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventEntity.Id, result.Id);
        Assert.Equal("Existing event", result.Title);
    }

    [Fact]
    public async Task GetById_UnknownEvent_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByFilter_Title_ReturnsEventsContainingTitle()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var matchingFirst = CreateEvent("DotNet meetup", DateTime.UtcNow.AddDays(1));
        var matchingSecond = CreateEvent("Advanced DotNet workshop", DateTime.UtcNow.AddDays(2));
        var other = CreateEvent("Java conference", DateTime.UtcNow.AddDays(3));

        context.Events.AddRange(matchingFirst, matchingSecond, other);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new EventRepository(context);

        // Act
        var result = (await repository.GetByFilter("DotNet", null, null, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Id == matchingFirst.Id);
        Assert.Contains(result, e => e.Id == matchingSecond.Id);
        Assert.DoesNotContain(result, e => e.Id == other.Id);
    }

    [Fact]
    public async Task GetByFilter_DateRange_ReturnsEventsInsideRange()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var baseDate = DateTime.UtcNow.Date.AddDays(10);
        var beforeRange = CreateEvent("Before range", baseDate.AddDays(-2));
        var insideRange = CreateEvent("Inside range", baseDate);
        var afterRange = CreateEvent("After range", baseDate.AddDays(4));

        context.Events.AddRange(beforeRange, insideRange, afterRange);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new EventRepository(context);

        // Act
        var result =
            (await repository.GetByFilter(null, baseDate.AddDays(-1), baseDate.AddDays(2), CancellationToken.None))
            .ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(insideRange.Id, result[0].Id);
    }

    [Fact]
    public async Task Delete_RemovesEventFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var eventEntity = CreateEvent("Event to delete", DateTime.UtcNow.AddDays(5));
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new EventRepository(context);

        // Act
        repository.Delete(eventEntity, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var deleted = await verifyContext.Events
            .FirstOrDefaultAsync(e => e.Id == eventEntity.Id, CancellationToken.None);

        Assert.Null(deleted);
    }

    private static Event CreateEvent(string title, DateTime startAt)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = $"{title} description",
            StartAt = startAt,
            EndAt = startAt.AddHours(2),
            TotalSeats = 100,
            AvailableSeats = 100
        };
    }
}