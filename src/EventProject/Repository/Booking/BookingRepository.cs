namespace EventProject.Repository.Booking;

public class BookingRepository : IBookingRepository
{
    private readonly List<Models.Booking> _booking = [];

    public Models.Booking? GetById(Guid id)
    {
        return _booking.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Models.Booking> GetAll()
    {
        return _booking;
    }

    public Models.Booking Add(Models.Booking entity)
    {
        _booking.Add(entity);
        return entity;
    }

    public Models.Booking Update(Guid id, Models.Booking entity)
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