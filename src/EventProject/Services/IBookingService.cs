using EventProject.Models;

namespace EventProject.Services;

public interface IBookingService
{
    Booking CreateBookingAsync(Guid eventId);
    Booking GetBookingByIdAsync(Guid bookingId);
}