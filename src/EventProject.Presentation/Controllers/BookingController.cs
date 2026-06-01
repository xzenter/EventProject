using EventProject.Domain.Entities;
using EventProject.Presentation.Services.Booking;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Presentation.Controllers;

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
    public async Task<ActionResult<Booking>> GetBooking(Guid id, CancellationToken ct = default)
    {
        var bookingInfo = await bookingService.GetBookingById(id, ct);

        return Ok(bookingInfo);
    }

    /// <summary>
    /// Создать бронирование для указанного события.
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <response code="202">Бронирование успешно создано.</response>
    /// <response code="404">Событие для бронирования не найдено.</response>
    /// <response code="409">Нет доступных мест на событие</response>
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost("/events/{id:guid}/book")]
    public async Task<IActionResult> CreateBooking(Guid id, CancellationToken ct = default)
    {
        var bookingInfo = await bookingService.CreateBooking(id, ct);

        return Accepted($"/bookings/{bookingInfo.Id}", bookingInfo);
    }
}