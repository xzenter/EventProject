using EventProject.Models;

namespace EventProject.Repository;

public interface IEventRepository
{
    Event? GetById(Guid id);
    IEnumerable<Event> GetAll();
    Event Add(Event entity);
    Event Update(Guid id, Event entity);
    void Delete(Guid id);
}