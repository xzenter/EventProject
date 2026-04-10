using EventProject.Models;

namespace EventProject.Repository;

public interface IBookingRepository
{
    Booking? GetById(Guid id);
    IEnumerable<Booking> GetAll();
    Booking Add(Booking entity);
    Booking Update(Guid id, Booking entity);
    void Delete(Guid id);
}