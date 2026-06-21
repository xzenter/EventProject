using EventProject.Domain.Entities;
using EventProject.Domain.Enums;

namespace EventProject.Application.Booking.DTOs;

public class BookingInfo
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; init; }
}