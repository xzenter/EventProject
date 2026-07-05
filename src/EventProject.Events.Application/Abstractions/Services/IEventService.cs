using EventProject.Events.Application.DTOs;
using EventProject.Events.Application.Events.DTOs;

namespace EventProject.Events.Application.Abstractions.Services;

public interface IEventService
{
    Task<PaginatedResult<EventDto>> GetEvents(SearchEventsQuery query, CancellationToken ct);
    Task<EventDto> GetEvent(Guid id, CancellationToken ct);
    Task<EventDto> CreateEvent(EventForCreationQuery query, CancellationToken ct);
    Task<EventDto> UpdateEvent(Guid id, EventForUpdateQuery query, CancellationToken ct);
    Task DeleteEvent(Guid id, CancellationToken ct);
}