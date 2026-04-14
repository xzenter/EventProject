namespace EventProject.Repository.Event;

public class EventRepository : IEventRepository
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Models.Event> _events = [];

    public Models.Event? GetById(Guid id)
    {
        _events.TryGetValue(id, out var eventEntity);
        return eventEntity;
    }

    public IEnumerable<Models.Event> GetAll()
    {
        return _events.Values;
    }

    public Models.Event Add(Models.Event entity)
    {
        _events[entity.Id] = entity;
        return entity;
    }

    public Models.Event Update(Guid id, Models.Event entity)
    {
        if (!_events.TryGetValue(id, out _))
        {
            throw new KeyNotFoundException($"Event with id {id} was not found.");
        }

        _events[id] = entity;
        return entity;
    }

    public void Delete(Guid id)
    {
        _events.TryRemove(id, out _);
    }
}
