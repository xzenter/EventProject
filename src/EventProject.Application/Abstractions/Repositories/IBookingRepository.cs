using EventProject.Domain.Entities;

namespace EventProject.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task<Domain.Entities.Booking?> GetById(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Domain.Entities.Booking>> GetByStatus(BookingStatus status, CancellationToken ct = default);

    Task Add(Domain.Entities.Booking entity, CancellationToken ct = default);

    Task<int> SaveChanges(CancellationToken ct = default);
}