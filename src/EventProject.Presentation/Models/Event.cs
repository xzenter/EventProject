namespace EventProject.Presentation.Models;

public class Event
{
    public Event()
    {
    }
    
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }

    public required int TotalSeats { get; set; }

    public int AvailableSeats { get; set; }

    /// <summary>
    /// Коллекция броней на событие
    /// </summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats >= count)
        {
            AvailableSeats -= count;

            return true;
        }

        return false;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats += count;

        if (AvailableSeats > TotalSeats)
        {
            AvailableSeats = TotalSeats;
        }
    }
}