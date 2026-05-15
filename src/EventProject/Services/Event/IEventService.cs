using EventProject.Dto;
using EventProject.Dto.Query;
using EventProject.Dto.Response;

namespace EventProject.Services.Event;

public interface IEventService
{
    Task<PaginatedResult<EventDto>> GetEvents(SearchEventsQuery query, CancellationToken ct);
    Task<EventDto> GetEvent(Guid id, CancellationToken ct);
    Task<EventDto> CreateEvent(EventForCreationQuery query, CancellationToken ct);
    Task<EventDto> UpdateEvent(Guid id, EventForUpdateQuery query, CancellationToken ct);
    Task DeleteEvent(Guid id, CancellationToken ct);
}