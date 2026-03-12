using EventProject.Controllers.Events.Dto;
using EventProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Controllers.Events;

[ApiController]
[Route("api/events")]
public class EventsController(IEventService eventService): ControllerBase
{
    // GET /events — получить список всех событий;
    [HttpGet]
    public IActionResult GetEvents()
    {
        return Ok();
    }
    // GET /events/{id} — получить событие по id; если не найдено — вернуть корректный HTTP-ответ (например, 404);
    [HttpGet("{id}")]
    public IActionResult GetEvent(int id)
    {
        return Ok();
    }
    // POST /events — создать событие, возвращать корректный HTTP-ответ (например, 201);
    [HttpPost]
    public IActionResult CreateEvent(EventForCreatioDto eventDto)
    {
        return Ok();
    }
    // PUT /events/{id} — обновить событие целиком; если не найдено — вернуть корректный HTTP-ответ (например, 404);
    [HttpPut("{id}")]
    public IActionResult UpdateEvent(int id, EventForUpdateDto eventDto)
    {
        return Ok();
    }
    // DELETE /events/{id} — удалить событие; если не найдено — вернуть корректный HTTP-ответ (например, 404).
    [HttpDelete("{id}")]
    public IActionResult DeleteEvent(int id)
    {
        return Ok();
    }
}