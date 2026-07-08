using EventProject.Users.Domain.Entities;

namespace EventProject.Users.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task AddUser(User user, CancellationToken ct);

    Task<User?> GetUser(Guid userId, CancellationToken ct);

    Task<User?> GetUserByLogin(string login, CancellationToken ct);

    Task<int> SaveChanges(CancellationToken ct = default);
}