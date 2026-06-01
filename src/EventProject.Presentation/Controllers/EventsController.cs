using EventProject.Presentation.Dto.Query;
using EventProject.Presentation.Dto.Response;
using EventProject.Presentation.Services.Event;
using EventProject.Presentation.Services.Booking;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Presentation.Controllers;

[ApiController]
[Route("events")]
[Produces("application/json")]
public class EventsController(
    IEventService eventService
) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    /// <param name="query">Параметры для поиска событий.</param>
    /// <response code="200">Запрос обработан.</response>
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetEvents([FromQuery] SearchEventsQuery query,
        CancellationToken ct = default)
    {
        var events = await eventService.GetEvents(query, ct);
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
    public async Task<ActionResult<EventDto>> GetEvent(Guid id, CancellationToken ct = default)
    {
        var eventDto = await eventService.GetEvent(id, ct);
        return Ok(eventDto);
    }

    /// <summary>
    /// Создать событие.
    /// </summary>
    /// <param name="eventForCreationQuery">Параметры события.</param>
    /// <response code="201">Событие создано.</response>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventForCreationQuery eventForCreationQuery,
        CancellationToken ct = default)
    {
        var newEventDto = await eventService.CreateEvent(eventForCreationQuery, ct);
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
    public async Task<IActionResult> UpdateEvent(Guid id, EventForUpdateQuery eventForUpdateQuery,
        CancellationToken ct = default)
    {
        var eventDto = await eventService.UpdateEvent(id, eventForUpdateQuery, ct);
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
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken ct = default)
    {
        await eventService.DeleteEvent(id, ct);
        return Ok();
    }
}