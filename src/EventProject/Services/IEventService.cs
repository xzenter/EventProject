using EventProject.Controllers;
using EventProject.Controllers.Events.Query;
using EventProject.Controllers.Events.Response;
using EventProject.Models;

namespace EventProject.Services;

public interface IEventService
{
    PaginatedResult<EventDto> GetEvents(SearchEventsQuery query);
    EventDto? GetEvent(Guid id);
    EventDto CreateEvent(EventForCreationQuery eventForCreationQuery);
    EventDto UpdateEvent(Guid id, EventForUpdateQuery eventForUpdateQuery);
    void DeleteEvent(Guid id);
}