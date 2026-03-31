using EventProject.Models;

namespace EventProject.Repository;

public class EventRepository : IEventRepository
{
    private readonly List<Event> Events = [];

    public Event? GetById(Guid id)
    {
        return Events.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Event> GetAll()
    {
        return Events;
    }

    public Event Add(Event entity)
    {
        Events.Add(entity);
        return entity;
    }

    public Event Update(Guid id, Event entity)
    {
        var index = Events.FindIndex(e => e.Id == id);
        Events[index] = entity;
        return entity;
    }

    public void Delete(Guid id)
    {
        var findEvent = Events.FirstOrDefault(e => e.Id == id);

        if (findEvent != null)
        {
            Events.Remove(findEvent);
        }
    }
}