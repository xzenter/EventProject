using EventProject.Dto;
using EventProject.Dto.Query;
using EventProject.Dto.Response;

namespace EventProject.Services.Event;

public interface IEventService
{
    PaginatedResult<EventDto> GetEvents(SearchEventsQuery query);
    EventDto GetEvent(Guid id);
    EventDto CreateEvent(EventForCreationQuery query);
    EventDto UpdateEvent(Guid id, EventForUpdateQuery query);
    void DeleteEvent(Guid id);
}