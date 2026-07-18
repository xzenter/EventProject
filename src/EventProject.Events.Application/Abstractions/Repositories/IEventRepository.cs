namespace EventProject.Events.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task<Domain.Entities.Event?> GetById(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Domain.Entities.Event>> GetByFilter(string? title, DateTime? from, DateTime? to, CancellationToken ct);

    Task<IReadOnlyCollection<Domain.Entities.Event>> GetTopEvents(int count = 10, CancellationToken ct = default);

    Task Add(Domain.Entities.Event entity, CancellationToken ct = default);

    void Delete(Domain.Entities.Event entity, CancellationToken ct = default);

    Task<int> SaveChanges(CancellationToken ct = default);
}