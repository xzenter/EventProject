using EventProject.Models;
using EventProject.Services.Booking;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Controllers;

[ApiController]
[Produces("application/json")]
public class BookingController(
    IBookingService bookingService
) : ControllerBase
{
    /// <summary>
    /// Получение брони по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <response code="200">Бронирование получено.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [ProducesResponseType(typeof(Booking), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("/bookings/{id:guid}")]
    public async Task<ActionResult<Booking>> GetBooking(Guid id)
    {
        var bookingInfo = await bookingService.GetBookingByIdAsync(id);

        return Ok(bookingInfo);
    }
}