using EventProject.Bookings.Application.Abstractions.Repositories;
using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EventProject.Bookings.Application.Services;

public class BookingConfirmProcessor
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingProducerService _bookingProducerService;
    private readonly ILogger<BookingConfirmProcessor> _logger;

    public BookingConfirmProcessor(
        IBookingRepository bookingRepository,
        IBookingProducerService bookingProducerService,
        ILogger<BookingConfirmProcessor> logger)
    {
        _bookingRepository = bookingRepository;
        _bookingProducerService = bookingProducerService;
        _logger = logger;
    }

    public async Task Execute(CancellationToken stoppingToken)
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
        // Имитация внешнего вызова
        await Task.Delay(10000, stoppingToken);

        var booking = await _bookingRepository.GetById(bookingId, stoppingToken);

        try
        {
            if (booking is null)
            {
                _logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            booking.Confirm(DateTime.UtcNow);

            await _bookingRepository.SaveChanges(stoppingToken);
            await _bookingProducerService.SendConfirm(booking);

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

                await _bookingRepository.SaveChanges(stoppingToken);
            }

            _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", bookingId);
        }
    }
}