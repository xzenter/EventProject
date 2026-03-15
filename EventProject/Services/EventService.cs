using AutoMapper;
using EventProject.Controllers.Events.Dto;
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

    public IEnumerable<EventDto> GetEvents()
    {
        return _mapper.Map<IEnumerable<EventDto>>(Events);
    }

    public EventDto? GetEvent(Guid id)
    {
        var @event = Events.FirstOrDefault(e => e.Id == id);

        return @event == null
            ? null
            : _mapper.Map<EventDto>(@event);
    }

    public EventDto CreateEvent(EventForCreationDto eventDto)
    {
        var @event = _mapper.Map<Event>(eventDto);

        @event.Id = Guid.NewGuid();

        Events.Add(@event);

        return _mapper.Map<EventDto>(@event);
    }

    public EventDto UpdateEvent(Guid id, EventForUpdateDto eventForUpdateDto)
    {
        var @event = Events.FirstOrDefault(e => e.Id == id);

        if (@event == null)
        {
            throw new Exception($"Событие с id = {id} не найдено");
        }

        @event = new Event
        {
            Id = id,
            Title = eventForUpdateDto.Title,
            Description = eventForUpdateDto.Description,
            StartAt = eventForUpdateDto.StartAt,
            EndAt = eventForUpdateDto.EndAt
        };

        return _mapper.Map<EventDto>(@event);
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