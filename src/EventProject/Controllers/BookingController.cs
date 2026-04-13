using EventProject.Dto.Response;
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
    /// Создать бронирование для указанного события.
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <response code="202">Бронирование успешно создано.</response>
    /// <response code="404">Событие для бронирования не найдено.</response>
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("/events/{id:guid}/book")]
    public async Task<IActionResult> CreateBooking(Guid id)
    {
        var booking = await bookingService.CreateBookingAsync(id);

        var bookingDto = new BookingDto()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return Accepted($"/bookings/{bookingDto.BookingId}", bookingDto);
    }

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
        var booking = await bookingService.GetBookingByIdAsync(id);

        var bookingDto = new BookingDto()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return Ok(bookingDto);
    }
}