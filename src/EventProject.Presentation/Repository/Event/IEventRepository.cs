namespace EventProject.Presentation.Repository.Event;

public interface IEventRepository
{
    Task<Models.Event?> GetById(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Models.Event>> GetByFilter(string? title, DateTime? from, DateTime? to, CancellationToken ct);

    Task Add(Models.Event entity, CancellationToken ct = default);

    void Delete(Models.Event entity, CancellationToken ct = default);

    Task<int> SaveChanges(CancellationToken ct = default);
}