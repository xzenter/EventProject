using EventProject.Application.Abstractions.Repositories;
using EventProject.Domain.Enums;
using EventProject.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _appDbContext;

    public BookingRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Domain.Entities.Booking?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _appDbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IEnumerable<Domain.Entities.Booking>> GetByStatus(BookingStatus status,
        CancellationToken ct = default)
    {
        return await _appDbContext.Bookings
            .Where(b => b.Status == status)
            .ToListAsync(ct);
    }

    public async Task Add(Domain.Entities.Booking entity, CancellationToken ct = default)
    {
        await _appDbContext.Bookings
            .AddAsync(entity, ct);
    }

    public Task<int> GetActiveBookingsCount(Guid userId, CancellationToken ct = default)
    {
        return _appDbContext.Bookings
            .CountAsync(
                b => b.UserId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed),
                ct);
    }

    public async Task<int> SaveChanges(CancellationToken ct = default)
    {
        return await _appDbContext
            .SaveChangesAsync(ct);
    }
}