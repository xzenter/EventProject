using EventProject.Domain.Entities;
using EventProject.Domain.Exceptions;
using EventProject.Presentation.Repository.Booking;
using EventProject.Presentation.Repository.Event;

namespace EventProject.Presentation.Services.Booking;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<BookingInfo> CreateBooking(Guid eventId, CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);

        try
        {
            var existingEvent = await _eventRepository.GetById(eventId, ct);
            if (existingEvent == null) throw new NotFoundException($"Событие с id = {eventId} не найдено");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException("No available seats for this event");

            var booking = new Domain.Entities.Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
                Event = existingEvent
            };

            await _bookingRepository.Add(booking, ct);
            await _bookingRepository.SaveChanges(ct);

            var bookingInfo = new BookingInfo
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status
            };

            return bookingInfo;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<BookingInfo> GetBookingById(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetById(bookingId, ct);

        if (booking == null)
        {
            throw new NotFoundException($"Бронирование с id = {bookingId} не найдено");
        }

        var bookingInfo = new BookingInfo
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status
        };

        return bookingInfo;
    }
}