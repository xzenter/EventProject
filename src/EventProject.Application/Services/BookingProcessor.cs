using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Abstractions.Services;
using EventProject.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EventProject.Application.Services;

public class BookingProcessor : IBookingProcessor
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<BookingProcessor> _logger;

    public BookingProcessor(
        IBookingRepository bookingRepository,
        IEventRepository eventRepository,
        ILogger<BookingProcessor> logger)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            var pendingBookings = await _bookingRepository.GetByStatus(BookingStatus.Pending, stoppingToken);

            var tasks = pendingBookings
                .Take(50)
                .Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса брони");
        }
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        // имитация внешнего вызова
        await Task.Delay(1000, stoppingToken);

        Domain.Entities.Event? @event = null;
        var booking = await _bookingRepository.GetById(bookingId, stoppingToken);

        try
        {
            if (booking is null)
            {
                _logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            @event = await _eventRepository.GetById(booking.EventId, stoppingToken);

            if (@event is null)
            {
                // Если событие не найдено, отклоняем бронь, так как она не может быть обработана без связанного события
                booking.Reject(DateTime.UtcNow);
                await _bookingRepository.SaveChanges(stoppingToken);
                await _eventRepository.SaveChanges(stoppingToken);

                _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.Id);

                return;
            }

            // Подтверждаем бронь
            booking.Confirm(DateTime.UtcNow);
            await _bookingRepository.SaveChanges(stoppingToken);
            await _eventRepository.SaveChanges(stoppingToken);

            _logger.LogInformation("Бронь {BookingId} обработана и подтверждена", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Обработка брони {BookingId} была отменена", bookingId);
        }
        catch (Exception)
        {
            if (booking is not null)
            {
                booking.Reject(DateTime.UtcNow);

                if (@event is not null)
                    @event.ReleaseSeats();

                await _bookingRepository.SaveChanges(stoppingToken);
                await _eventRepository.SaveChanges(stoppingToken);
            }

            _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", bookingId);
        }
    }
}