namespace EventProject.Services.Booking;

public interface IBookingService
{
    Task<Models.Booking> CreateBookingAsync(Guid eventId);
    Task<Models.Booking> GetBookingByIdAsync(Guid bookingId);
}