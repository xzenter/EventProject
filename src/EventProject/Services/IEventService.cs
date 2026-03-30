using EventProject.Controllers.Events.Dto;

namespace EventProject.Services;

public interface IEventService
{
    IEnumerable<EventDto> GetEvents();
    IEnumerable<EventDto> GetEvents(string? title, DateTime? from, DateTime? to);
    EventDto? GetEvent(Guid id);
    EventDto CreateEvent(EventForCreationDto eventForCreationDto);
    EventDto UpdateEvent(Guid id, EventForUpdateDto eventForUpdateDto);
    void DeleteEvent(Guid id);
}