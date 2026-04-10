namespace EventProject.Repository.Event;

public class EventRepository : IEventRepository
{
    private readonly List<Models.Event> _events = [];

    public Models.Event? GetById(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Models.Event> GetAll()
    {
        return _events;
    }

    public Models.Event Add(Models.Event entity)
    {
        _events.Add(entity);
        return entity;
    }

    public Models.Event Update(Guid id, Models.Event entity)
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