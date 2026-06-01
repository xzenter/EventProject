using EventProject.Domain.Entities;

namespace EventProject.Presentation.Services.Booking;

public interface IBookingService
{
    Task<BookingInfo> CreateBooking(Guid eventId, CancellationToken ct);
    Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct);
}