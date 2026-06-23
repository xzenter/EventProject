using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Abstractions.Services;
using EventProject.Application.Booking.DTOs;
using EventProject.Domain.Enums;
using EventProject.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace EventProject.Application.Booking;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly BookingSettings _bookingSettings;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public BookingService(
        IBookingRepository bookingRepository,
        IEventRepository eventRepository,
        IOptions<BookingSettings> bookingSettings
    )
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _bookingSettings = bookingSettings.Value;
    }

    public async Task<BookingInfo> CreateBooking(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);

        try
        {
            var existingEvent = await _eventRepository.GetById(eventId, ct);

            if (existingEvent == null)
                throw new NotFoundException($"Событие по идентификатору {eventId} не найдено");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException("Недостаточно свободных мест");

            var activeBookingsCount = await _bookingRepository.GetActiveBookingsCount(userId, ct);

            if (activeBookingsCount >= _bookingSettings.MaxActiveBookings)
                throw new ActiveBookingLimitExceededException($"Пользователь не может иметь более " +
                                                              $"{_bookingSettings.MaxActiveBookings} активных броней.");

            var booking = new Domain.Entities.Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
                Event = existingEvent
            };

            await _bookingRepository.Add(booking, ct);
            await _bookingRepository.SaveChanges(ct);

            var bookingInfo = new BookingInfo
            {
                BookingId = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
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
            throw new NotFoundException($"Бронирование по идентификатору {bookingId} не найдено");

        var bookingInfo = new BookingInfo
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            UserId = booking.UserId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };

        return bookingInfo;
    }

    public async Task CancelBooking(Guid bookingId, Guid userId, Role role, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetById(bookingId, ct);

        if (booking is null)
            throw new NotFoundException($"Бронирование по идентификатору {bookingId} не найдено");

        if (role != Role.Admin && booking.UserId != userId)
            throw new BookingAccessDeniedException("Недостаточно прав на выполнение данной операции");

        if (booking.Status is BookingStatus.Cancelled)
            throw new BadRequestException("Бронь уже отменена");

        var @event = await _eventRepository.GetById(booking.EventId, ct);

        if (@event is null)
            throw new NotFoundException($"Событие по идентификатору {booking.EventId} не найдено");

        if (role != Role.Admin && @event.StartAt <= DateTime.UtcNow)
            throw new EventAlreadyStartedException("Невозможно отменить бронь для завершенного события");

        booking.Cancel(DateTime.UtcNow);
        @event.ReleaseSeats();

        await _bookingRepository.SaveChanges(ct);
    }
}