namespace EventProject.Presentation.Models;

public class BookingInfo
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; init; }
}