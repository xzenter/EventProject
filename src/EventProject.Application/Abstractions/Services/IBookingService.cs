using EventProject.Application.Booking.DTOs;
using EventProject.Domain.Entities;

namespace EventProject.Application.Abstractions.Services;

public interface IBookingService
{
    Task<BookingInfo> CreateBooking(Guid eventId, CancellationToken ct);
    Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct);
}