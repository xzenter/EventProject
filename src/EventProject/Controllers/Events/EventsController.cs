using EventProject.Controllers.Events.Dto;
using EventProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Controllers.Events;

[ApiController]
[Route("events")]
[Produces("application/json")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    /// <param name="title">Название события.</param>
    /// <param name="from">Дата начала события.</param>
    /// <param name="to">Дата окончания события.</param>
    /// <response code="200">Запрос обработан.</response>
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public ActionResult<List<EventDto>> GetEvents(string? title, DateTime? from, DateTime? to)
    {
        var events = eventService.GetEvents(title, from, to);

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
    /// <response code="201">Событие создано.</response>
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
    /// <response code="200">Событие успешно обновлено.</response>
    /// <response code="404">Событие для обновления не найдено.</response>
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
    /// <response code="200">Событие успешно удалено.</response>
    /// <response code="404">Событие для удаления не найдено.</response>
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