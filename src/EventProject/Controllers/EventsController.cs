using EventProject.Dto.Query;
using EventProject.Dto.Response;
using EventProject.Services.Booking;
using EventProject.Services.Event;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Controllers;

[ApiController]
[Route("events")]
[Produces("application/json")]
public class EventsController(
    IEventService eventService,
    IBookingService bookingService
) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    /// <param name="query">Параметры для поиска событий.</param>
    /// <response code="200">Запрос обработан.</response>
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public ActionResult<List<EventDto>> GetEvents([FromQuery] SearchEventsQuery query)
    {
        var events = eventService.GetEvents(query);
        return Ok(events);
    }

    /// <summary>
    /// Получить событие по id.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <response code="200">Событие получено.</response>
    /// <response code="404">Событие не найдено.</response>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public ActionResult<EventDto> GetEvent(Guid id)
    {
        var eventDto = eventService.GetEvent(id);
        return Ok(eventDto);
    }

    /// <summary>
    /// Создать событие.
    /// </summary>
    /// <param name="eventForCreationQuery">Параметры события.</param>
    /// <response code="201">Событие создано.</response>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [HttpPost]
    public IActionResult CreateEvent(EventForCreationQuery eventForCreationQuery)
    {
        var newEventDto = eventService.CreateEvent(eventForCreationQuery);
        return CreatedAtAction(nameof(GetEvent), new { id = newEventDto.Id }, newEventDto);
    }

    /// <summary>
    /// Обновить событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="eventForUpdateQuery">Параметры для обновления.</param>
    /// <response code="200">Событие успешно обновлено.</response>
    /// <response code="404">Событие для обновления не найдено.</response>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent(Guid id, EventForUpdateQuery eventForUpdateQuery)
    {
        var eventDto = eventService.UpdateEvent(id, eventForUpdateQuery);
        return Ok(eventDto);
    }

    /// <summary>
    /// Удалить событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <response code="204">Событие успешно удалено.</response>
    /// <response code="404">Событие для удаления не найдено.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent(Guid id)
    {
        eventService.DeleteEvent(id);
        return Ok();
    }

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
        var bookingInfo = await bookingService.CreateBookingAsync(id);

        return Accepted($"/bookings/{bookingInfo.Id}", bookingInfo);
    }
}