namespace EventProject.Events.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task<Domain.Entities.Event?> GetById(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Domain.Entities.Event>> GetByFilter(string? title, DateTime? from, DateTime? to, CancellationToken ct);

    Task Add(Domain.Entities.Event entity, CancellationToken ct = default);

    void Delete(Domain.Entities.Event entity, CancellationToken ct = default);

    Task<int> SaveChanges(CancellationToken ct = default);
}