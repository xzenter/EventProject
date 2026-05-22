using EventProject.DataAccess;
using EventProject.Models;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Repository.Booking;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _appDbContext;

    public BookingRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Models.Booking?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _appDbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IEnumerable<Models.Booking>> GetByStatus(BookingStatus status, CancellationToken ct = default)
    {
        return await _appDbContext.Bookings
            .Where(b => b.Status == status)
            .ToListAsync(ct);
    }

    public async Task Add(Models.Booking entity, CancellationToken ct = default)
    {
        await _appDbContext.Bookings
            .AddAsync(entity, ct);
    }

    public async Task<int> SaveChanges(CancellationToken ct = default)
    {
        return await _appDbContext
            .SaveChangesAsync(ct);
    }
}
