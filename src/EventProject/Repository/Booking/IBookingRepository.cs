using EventProject.Models;

namespace EventProject.Repository.Booking;

public interface IBookingRepository
{
    Task<Models.Booking?> GetById(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Models.Booking>> GetByStatus(BookingStatus status, CancellationToken ct = default);

    Task Add(Models.Booking entity, CancellationToken ct = default);

    Task<int> SaveChanges(CancellationToken ct = default);
}