using EventProject.Bookings.Application.Abstractions.Repositories;
using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Application.Booking.DTOs;
using EventProject.Bookings.Domain.Enums;
using EventProject.Bookings.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace EventProject.Bookings.Application.Booking;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly BookingSettings _bookingSettings;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public BookingService(
        IBookingRepository bookingRepository,
        IOptions<BookingSettings> bookingSettings
    )
    {
        _bookingRepository = bookingRepository;
        _bookingSettings = bookingSettings.Value;
    }

    public async Task<BookingInfo> CreateBooking(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        await Semaphore.WaitAsync(ct);

        try
        {
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

        booking.Cancel(DateTime.UtcNow);

        await _bookingRepository.SaveChanges(ct);
    }
}