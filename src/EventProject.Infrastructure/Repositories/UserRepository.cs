using EventProject.Application.Abstractions.Repositories;
using EventProject.Domain.Entities;
using EventProject.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task AddUser(User user, CancellationToken ct)
    {
        await _appDbContext.Users
            .AddAsync(user, ct);
    }

    public async Task<User?> GetUserByLogin(string login, CancellationToken ct)
    {
        return await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Login == login, ct);
    }

    public async Task<int> SaveChanges(CancellationToken ct = default)
    {
        return await _appDbContext
            .SaveChangesAsync(ct);
    }
}