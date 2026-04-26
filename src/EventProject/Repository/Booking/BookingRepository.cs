using EventProject.Models;

namespace EventProject.Repository.Booking;

public class BookingRepository : IBookingRepository
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Models.Booking> _booking = [];

    public Models.Booking? GetById(Guid id)
    {
        _booking.TryGetValue(id, out var booking);
        return booking;
    }

    public IEnumerable<Models.Booking> GetByStatus(BookingStatus status)
    {
        return _booking.Values.Where(x => x.Status == status);
    }

    public Models.Booking Add(Models.Booking entity)
    {
        _booking[entity.Id] = entity;
        return entity;
    }

    public Models.Booking Update(Guid id, Models.Booking entity)
    {
        if (!_booking.TryGetValue(id, out _))
        {
            throw new KeyNotFoundException($"Booking with id {id} was not found.");
        }

        _booking[id] = entity;
        return entity;
    }

    public void Delete(Guid id)
    {
        _booking.TryRemove(id, out _);
    }
}