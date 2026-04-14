using EventProject.Models;

namespace EventProject.Services.Booking;

public interface IBookingService
{
    Task<BookingInfo> CreateBookingAsync(Guid eventId);
    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);
}