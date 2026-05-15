namespace EventProject.Models;

public class Booking
{
    public Booking()
    {
    }
    
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Событие, к которому относится бронь
    /// </summary>
    public required Event Event { get; set; }


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
}