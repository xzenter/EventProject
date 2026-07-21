using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Domain.Entities;
using EventProject.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Events.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _appDbContext;

    public EventRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Event?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IEnumerable<Event>> GetByFilter(string? title, DateTime? from, DateTime? to,
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

    public async Task<IReadOnlyCollection<Event>> GetTopEvents(int count = 10, CancellationToken ct = default)
    {
        return await _appDbContext.Events.AsNoTracking()
            .Where(e => e.AvailableSeats < e.TotalSeats)
            .OrderByDescending(e => (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task Add(Event entity, CancellationToken ct = default)
    {
        await _appDbContext.Events.AddAsync(entity, ct);
    }

    public void Delete(Event entity, CancellationToken ct = default)
    {
        _appDbContext.Events.Remove(entity);
    }

    public async Task<int> SaveChanges(CancellationToken ct = default)
    {
        return await _appDbContext
            .SaveChangesAsync(ct);
    }
}