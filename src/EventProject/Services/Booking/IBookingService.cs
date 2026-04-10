using EventProject.Dto.Response;

namespace EventProject.Services.Booking;

public interface IBookingService
{
    BookingDto CreateBookingAsync(Guid eventId);
    BookingDto GetBookingByIdAsync(Guid bookingId);
}