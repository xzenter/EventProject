using EventProject.Application.Abstractions.Services;
using EventProject.Application.Booking.DTOs;
using EventProject.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Presentation.Controllers;

[ApiController]
[Authorize]
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
    [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("/bookings/{id:guid}")]
    public async Task<ActionResult<BookingInfo>> GetBooking(Guid id, CancellationToken ct = default)
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
    /// <response code="409">Нет доступных мест на событие.</response>
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost("/events/{id:guid}/book")]
    public async Task<IActionResult> CreateBooking(Guid id, CancellationToken ct = default)
    {
        var user = HttpContext.GetUser();
        var bookingInfo = await bookingService.CreateBooking(id, user.UserId, ct);
        return Accepted($"/bookings/{bookingInfo.BookingId}", bookingInfo);
    }

    /// <summary>
    /// Отменить бронирование.
    /// </summary>
    /// <param name="id">Идентификатор события для дальнейшей отмены.</param>
    /// <response code="204">Бронь успешно отменена.</response>
    /// <response code="400">Событие уже началось или окончено или бронь уже отменена.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="404">Бронь не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpDelete("bookings/{id}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken = default)
    {
        var user = HttpContext.GetUser();
        await bookingService.CancelBookingAsync(id, user.UserId, user.Role, cancellationToken);
        return NoContent();
    }
}