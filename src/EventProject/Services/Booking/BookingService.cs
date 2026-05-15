using EventProject.DataAccess;
using EventProject.Exceptions;
using EventProject.Models;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Services.Booking;

public class BookingService : IBookingService
{
    private readonly AppDbContext _appDbContext;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public BookingService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<BookingInfo> CreateBooking(Guid eventId, CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);

        try
        {
            var existingEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
            if (existingEvent == null) throw new NotFoundException($"Событие с id = {eventId} не найдено");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException("No available seats for this event");

            var booking = new Models.Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
                Event = existingEvent
            };

            await _appDbContext.Bookings.AddAsync(booking, ct);
            await _appDbContext.SaveChangesAsync(ct);

            var bookingInfo = new BookingInfo
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status
            };

            return bookingInfo;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _appDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking == null)
        {
            throw new NotFoundException($"Бронирование с id = {bookingId} не найдено");
        }

        var bookingInfo = new BookingInfo
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return bookingInfo;
    }
}