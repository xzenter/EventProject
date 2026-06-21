using EventProject.Application.Booking.DTOs;
using EventProject.Domain.Enums;

namespace EventProject.Application.Abstractions.Services;

public interface IBookingService
{
    Task<BookingInfo> CreateBooking(Guid eventId, Guid userId, CancellationToken ct);
    Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct);
    Task CancelBookingAsync(Guid bookingId, Guid userId, Role role, CancellationToken ct);
}