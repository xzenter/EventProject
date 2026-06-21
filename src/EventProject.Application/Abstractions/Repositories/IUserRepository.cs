namespace EventProject.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task AddUser(Domain.Entities.User user, CancellationToken ct);

    Task<Domain.Entities.User?> GetUserByLogin(string login, CancellationToken ct);

    Task<int> SaveChanges(CancellationToken ct = default);
}