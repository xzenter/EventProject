using EventProject.Models;

namespace EventProject.Repository;

public class BookingRepository : IBookingRepository
{
    private readonly List<Booking> _booking = [];

    public Booking? GetById(Guid id)
    {
        return _booking.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Booking> GetAll()
    {
        return _booking;
    }

    public Booking Add(Booking entity)
    {
        _booking.Add(entity);
        return entity;
    }

    public Booking Update(Guid id, Booking entity)
    {
        var index = _booking.FindIndex(e => e.Id == id);
        _booking[index] = entity;
        return entity;
    }

    public void Delete(Guid id)
    {
        var findEvent = _booking.FirstOrDefault(e => e.Id == id);

        if (findEvent != null)
        {
            _booking.Remove(findEvent);
        }
    }
}