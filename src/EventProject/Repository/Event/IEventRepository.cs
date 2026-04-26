namespace EventProject.Repository.Event;

public interface IEventRepository
{
    Models.Event? GetById(Guid id);
    IEnumerable<Models.Event> GetAll();
    Models.Event Add(Models.Event entity);
    Models.Event Update(Guid id, Models.Event entity);
    void Delete(Guid id);
}