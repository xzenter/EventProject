namespace EventProject.Repository.Booking;

public interface IBookingRepository
{
    Models.Booking? GetById(Guid id);
    IEnumerable<Models.Booking> GetAll();
    Models.Booking Add(Models.Booking entity);
    Models.Booking Update(Guid id, Models.Booking entity);
    void Delete(Guid id);
}