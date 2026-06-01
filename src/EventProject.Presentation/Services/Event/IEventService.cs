using EventProject.Presentation.Dto;
using EventProject.Presentation.Dto.Query;
using EventProject.Presentation.Dto.Response;

namespace EventProject.Presentation.Services.Event;

public interface IEventService
{
    Task<PaginatedResult<EventDto>> GetEvents(SearchEventsQuery query, CancellationToken ct);
    Task<EventDto> GetEvent(Guid id, CancellationToken ct);
    Task<EventDto> CreateEvent(EventForCreationQuery query, CancellationToken ct);
    Task<EventDto> UpdateEvent(Guid id, EventForUpdateQuery query, CancellationToken ct);
    Task DeleteEvent(Guid id, CancellationToken ct);
}