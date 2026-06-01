using EventProject.Presentation.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Presentation.Repository.Event;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _appDbContext;

    public EventRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Models.Event?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IEnumerable<Models.Event>> GetByFilter(string? title, DateTime? from, DateTime? to,
        CancellationToken ct)
    {
        var query = _appDbContext.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(e => e.Title.Contains(title));

        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from);

        if (to.HasValue)
            query = query.Where(e => e.EndAt <= to);

        return await query.ToListAsync(ct);
    }

    public async Task Add(Models.Event entity, CancellationToken ct = default)
    {
        await _appDbContext.Events.AddAsync(entity, ct);
    }

    public void Delete(Models.Event entity, CancellationToken ct = default)
    {
        _appDbContext.Events.Remove(entity);
    }

    public async Task<int> SaveChanges(CancellationToken ct = default)
    {
        return await _appDbContext
            .SaveChangesAsync(ct);
    }
}