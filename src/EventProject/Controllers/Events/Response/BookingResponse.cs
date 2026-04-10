using EventProject.Models;

namespace EventProject.Controllers.Events.Response;

public class CreateBookingResponse
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
}