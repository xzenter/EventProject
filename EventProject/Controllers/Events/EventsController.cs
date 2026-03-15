using EventProject.Controllers.Events.Dto;
using EventProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Controllers.Events;

[ApiController]
[Route("events")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public ActionResult<List<EventDto>> GetEvents()
    {
        var events = eventService.GetEvents();

        return Ok(events);
    }

    /// <summary>
    /// Получить событие по id.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public ActionResult<EventDto> GetEvent(Guid id)
    {
        var eventDto = eventService.GetEvent(id);

        if (eventDto == null)
        {
            return NotFound();
        }

        return Ok(eventDto);
    }

    /// <summary>
    /// Создать событие.
    /// </summary>
    /// <param name="eventForCreationDto">Параметры события.</param>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [HttpPost]
    public IActionResult CreateEvent(EventForCreationDto eventForCreationDto)
    {
        var newEventDto = eventService.CreateEvent(eventForCreationDto);

        return CreatedAtAction(nameof(GetEvent), new { id = newEventDto.Id }, newEventDto);
    }

    /// <summary>
    /// Обновить событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="eventForUpdateDto">Параметры для обновления.</param>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent(Guid id, EventForUpdateDto eventForUpdateDto)
    {
        var eventDto = eventService.GetEvent(id);

        if (eventDto == null)
        {
            return NotFound();
        }

        eventDto = eventService.UpdateEvent(id, eventForUpdateDto);

        return Ok(eventDto);
    }

    /// <summary>
    /// Удалить событие.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent(Guid id)
    {
        var eventDto = eventService.GetEvent(id);

        if (eventDto == null)
        {
            return NotFound();
        }

        eventService.DeleteEvent(id);

        return Ok();
    }
}