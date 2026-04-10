using EventProject.Models;

namespace EventProject.Repository;

public class EventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    public Event? GetById(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Event> GetAll()
    {
        return _events;
    }

    public Event Add(Event entity)
    {
        _events.Add(entity);
        return entity;
    }

    public Event Update(Guid id, Event entity)
    {
        var index = _events.FindIndex(e => e.Id == id);
        _events[index] = entity;
        return entity;
    }

    public void Delete(Guid id)
    {
        var findEvent = _events.FirstOrDefault(e => e.Id == id);

        if (findEvent != null)
        {
            _events.Remove(findEvent);
        }
    }
}