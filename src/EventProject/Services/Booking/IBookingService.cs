using EventProject.Dto.Response;

namespace EventProject.Services.Booking;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId);
    Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
}