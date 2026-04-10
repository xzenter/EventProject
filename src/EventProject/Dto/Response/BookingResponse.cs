using EventProject.Models;

namespace EventProject.Dto.Response;

public class BookingDto
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
}