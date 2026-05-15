using EventProject.BackgroundServices;
using EventProject.DataAccess;
using EventProject.Dto.Query;
using EventProject.Exceptions;
using EventProject.Models;
using EventProject.Services.Booking;
using EventProject.Services.Event;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventProject.Tests;

public class BookingServiceInMemoryTests
{
    private readonly ServiceProvider _serviceProvider;

    public BookingServiceInMemoryTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<EventService>();
        services.AddScoped<BookingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    // Создание брони для существующего события — возвращается BookingInfo со статусом Pending
    [Fact]
    public async Task CreateBooking_ForExistingEvent_ShouldReturnBookingWithPendingStatus()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventDto = await eventService.CreateEvent(new EventForCreationQuery
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 1
        });

        // Act
        var result = await bookingService.CreateBooking(eventDto.Id);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventDto.Id);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    // Создание нескольких броней для одного события — все создаются с уникальными Id
    [Fact]
    public async Task CreateMultipleBookings_ForSameEvent_ShouldHaveUniqueIds()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 2,
            AvailableSeats = 2
        });

        await context.SaveChangesAsync();

        // Act
        var booking1 = await bookingService.CreateBooking(eventId);
        var booking2 = await bookingService.CreateBooking(eventId);

        // Assert
        booking1.Id.Should().NotBe(booking2.Id);
    }

    // Получение брони по Id — возвращается корректная информация
    [Fact]
    public async Task GetBooking_ById_ShouldReturnCorrectInfo()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        context.Bookings.Add(new Booking
        {
            Id = bookingId,
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = default,
            ProcessedAt = null,
            Event = null
        });

        await context.SaveChangesAsync();

        // Act
        var result = await bookingService.GetBookingById(bookingId);

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
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 2,
            AvailableSeats = 2
        });

        context.Bookings.Add(new Booking
        {
            Id = bookingId,
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = default,
            ProcessedAt = null,
            Event = null
        });

        await context.SaveChangesAsync();

        // Act
        var factory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var bookingProcessingService =
            new BookingProcessingService(factory, NullLogger<BookingProcessingService>.Instance);
        await bookingProcessingService.StartAsync(CancellationToken.None);

        await Task.Delay(5000);

        // Assert
        var updatedBooking = await context.Bookings.AsNoTracking().FirstAsync();
        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    // Создание брони для несуществующего события
    [Fact]
    public async Task CreateBooking_ForNonExistentEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();

        // Act
        var action = () => bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Создание брони для удалённого события
    [Fact]
    public async Task CreateBooking_ForDeletedEvent_ShouldThrowNotFoundException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();

        // Act
        var action = () => bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Получение брони по несуществующему Id
    [Fact]
    public async Task GetBooking_ForNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var bookingId = Guid.NewGuid();

        // Act
        var action = () => bookingService.GetBookingById(bookingId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // Создание брони уменьшает AvailableSeats на 1.
    [Fact]
    public async Task CreateBooking_ShouldDecreaseAvailableSeatsByOne()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

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

        context.Events.Add(eventEntity);

        await context.SaveChangesAsync();

        // Act
        await bookingService.CreateBooking(eventId);

        // Assert
        eventEntity.AvailableSeats.Should().Be(initialAvailableSeats - 1);
    }

    // Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.
    [Fact]
    public async Task CreateMultipleBookings_UpToLimit_ShouldSucceedWithUniqueIds()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();
        const int totalSeats = 3;

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        });

        await context.SaveChangesAsync();

        // Act
        var bookings = new List<BookingInfo>();
        for (var i = 0; i < totalSeats; i++)
        {
            var booking = await bookingService.CreateBooking(eventId);
            bookings.Add(booking);
        }

        // Assert
        bookings.Should().HaveCount(totalSeats);
        bookings.Select(b => b.Id).Should().OnlyHaveUniqueItems();

        // Проверяем, что все брони имеют статус Pending
        foreach (var booking in bookings) booking.Status.Should().Be(BookingStatus.Pending);

        // Проверяем, что AvailableSeats уменьшилось до 0
        var eventEntity = await context.Events.FindAsync(eventId);
        eventEntity.Should().NotBeNull();
        eventEntity.AvailableSeats.Should().Be(0);
    }

    // После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException
    [Fact]
    public async Task CreateBooking_AfterSeatsExhausted_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();
        const int totalSeats = 1;

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = totalSeats,
            AvailableSeats = 0 // Все места уже заняты
        });

        await context.SaveChangesAsync();

        // Act
        var action = () => bookingService.CreateBooking(eventId);

        // Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    // Бронирование при отсутствии мест → NoAvailableSeatsException
    [Fact]
    public async Task CreateBooking_WhenNoSeatsAvailable_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = 0,
            AvailableSeats = 0
        });

        await context.SaveChangesAsync();

        // Act
        var action = () => bookingService.CreateBooking(eventId);

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
            Event = null
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
            Event = null
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
        using var scope1 = _serviceProvider.CreateScope();
        var context1 = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventId = Guid.NewGuid();

        context1.Events.Add(new Event
        {
            Id = eventId,
            Title = "TestEvent",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 5,
            AvailableSeats = 5
        });

        await context1.SaveChangesAsync();

        // Act
        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                using var scope2 = _serviceProvider.CreateScope();
                var bookingService = scope2.ServiceProvider.GetRequiredService<BookingService>();

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
        using var scope3 = _serviceProvider.CreateScope();
        var context3 = scope3.ServiceProvider.GetRequiredService<AppDbContext>();

        context3.Events.Single(e => e.Id == eventId).AvailableSeats.Should().Be(0);
    }

    // Тест на уникальность Id при конкурентных запросах
    [Fact]
    public async Task CreateBooking_WithConcurrentRequests_ShouldHaveUniqueIds()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();

        var eventId = Guid.NewGuid();
        const int totalSeats = 10;

        context.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        });

        await context.SaveChangesAsync();

        // Act
        var tasks = Enumerable.Range(0, totalSeats)
            .Select(_ => bookingService.CreateBooking(eventId))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var bookings = tasks.Select(t => t.Result).ToList();
        bookings.Select(b => b.Id).Should().OnlyHaveUniqueItems();
    }
}