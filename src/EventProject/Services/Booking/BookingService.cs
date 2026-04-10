using EventProject.Dto.Response;
using EventProject.Exceptions;
using EventProject.Models;
using EventProject.Repository.Booking;
using EventProject.Repository.Event;

namespace EventProject.Services.Booking;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public BookingDto CreateBookingAsync(Guid eventId)
    {
        var findEvent = _eventRepository.GetById(eventId);
        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {eventId} не найдено");
        }

        var booking = new Models.Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
            ProcessedAt = null
        };

        _bookingRepository.Add(booking);

        var bookingDto = new BookingDto()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return bookingDto;
    }

    public BookingDto GetBookingByIdAsync(Guid bookingId)
    {
        var booking = _bookingRepository.GetById(bookingId);

        if (booking == null)
        {
            throw new NotFoundException($"Бронирование с id = {bookingId} не найдено");
        }

        var bookingDto = new BookingDto()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return bookingDto;
    }
}