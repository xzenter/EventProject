using AutoMapper;
using EventProject.Controllers;
using EventProject.Controllers.Events.Query;
using EventProject.Controllers.Events.Response;
using EventProject.Models;

namespace EventProject.Services;

public class EventService : IEventService
{
    private readonly IMapper _mapper;
    private static readonly List<Event> Events = [];

    public EventService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public PaginatedResult<EventDto> GetEvents(SearchEventsQuery query)
    {
        var filtered = Events
            .Where(e =>
                (query.Title == null || e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase)) &&
                (query.From == null || e.StartAt >= query.From) &&
                (query.To == null || e.EndAt <= query.To)
            );

        var filteredCount = filtered.Count();
        var items = filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => _mapper.Map<EventDto>(e))
            .ToList();

        var totalPages = (int)Math.Ceiling((double)filteredCount / query.PageSize);

        return new PaginatedResult<EventDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = filteredCount,
            TotalPages = totalPages
        };
    }

    public EventDto? GetEvent(Guid id)
    {
        var @event = Events.FirstOrDefault(e => e.Id == id);

        return @event == null
            ? null
            : _mapper.Map<EventDto>(@event);
    }

    public EventDto CreateEvent(EventForCreationQuery eventForCreationQuery)
    {
        var @event = _mapper.Map<Event>(eventForCreationQuery);

        @event.Id = Guid.NewGuid();

        Events.Add(@event);

        return _mapper.Map<EventDto>(@event);
    }

    public EventDto UpdateEvent(Guid id, EventForUpdateQuery eventForUpdateQuery)
    {
        var @event = Events.FirstOrDefault(e => e.Id == id);

        if (@event == null)
        {
            throw new Exception($"Событие с id = {id} не найдено");
        }

        var index = Events.IndexOf(@event);
        Events[index] = new Event
        {
            Id = id,
            Title = eventForUpdateQuery.Title,
            Description = eventForUpdateQuery.Description,
            StartAt = eventForUpdateQuery.StartAt,
            EndAt = eventForUpdateQuery.EndAt
        };

        return _mapper.Map<EventDto>(Events[index]);
    }

    public void DeleteEvent(Guid id)
    {
        var @event = Events.FirstOrDefault(e => e.Id == id);

        if (@event == null)
        {
            return;
        }

        if (!Events.Remove(@event))
        {
            throw new Exception($"Событие с id = {id} не удалось удалить");
        }
    }
}