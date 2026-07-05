using EventProject.Bookings.Domain.Enums;

namespace EventProject.Bookings.Domain.Entities;

public class Booking
{
    public Booking()
    {
    }
    
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required Guid UserId { get; set; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }

    public void Reject(DateTime processedAt)
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = processedAt;
    }

    public void Confirm(DateTime processedAt)
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = processedAt;
    }
    
    public void Cancel(DateTime processedAt)
    {
        Status = BookingStatus.Cancelled;
        ProcessedAt = processedAt;
    }
}