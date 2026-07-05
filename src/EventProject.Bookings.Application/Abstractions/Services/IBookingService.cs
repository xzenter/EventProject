using EventProject.Bookings.Application.Booking.DTOs;
using EventProject.Bookings.Domain.Enums;

namespace EventProject.Bookings.Application.Abstractions.Services;

public interface IBookingService
{
    Task<BookingInfo> CreateBooking(Guid eventId, Guid userId, CancellationToken ct);
    Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct);
    Task CancelBooking(Guid bookingId, Guid userId, Role role, CancellationToken ct);
}