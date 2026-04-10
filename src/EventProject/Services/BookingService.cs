using EventProject.Exceptions;
using EventProject.Models;
using EventProject.Repository;

namespace EventProject.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public Booking CreateBookingAsync(Guid eventId)
    {
        var findEvent = _eventRepository.GetById(eventId);
        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {eventId} не найдено");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
            ProcessedAt = null
        };

        _bookingRepository.Add(booking);

        return booking;
    }

    public Booking GetBookingByIdAsync(Guid bookingId)
    {
        var booking = _bookingRepository.GetById(bookingId);

        if (booking == null)
        {
            throw new NotFoundException($"Бронирование с id = {bookingId} не найдено");
        }

        return booking;
    }
}