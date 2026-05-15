using EventProject.Models;

namespace EventProject.Services.Booking;

public interface IBookingService
{
    Task<BookingInfo> CreateBooking(Guid eventId, CancellationToken ct);
    Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct);
}