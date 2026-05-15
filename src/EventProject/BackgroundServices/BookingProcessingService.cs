using EventProject.DataAccess;
using EventProject.Models;
using EventProject.Repository.Booking;
using EventProject.Repository.Event;
using Microsoft.EntityFrameworkCore;

namespace EventProject.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _factory;
    private readonly ILogger<BookingProcessingService> _logger;

    public BookingProcessingService(IServiceScopeFactory factory, ILogger<BookingProcessingService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис обработки брони запущен - {Time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _factory.CreateScope();
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pendingBookings = await appDbContext.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .Take(50) // Ограничить количество обрабатываемых броней
                    .ToListAsync(stoppingToken);

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении статуса брони");
            }

            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Сервис управления обработкой брони остановлен");
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        using var scope = _factory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // имитация внешнего вызова
        await Task.Delay(1000, stoppingToken);

        Event? @event = null;
        var booking = await appDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, stoppingToken);

        try
        {
            if (booking is null)
            {
                _logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            @event = await appDbContext.Events.FirstOrDefaultAsync(e => e.Id == booking.EventId, stoppingToken);

            if (@event is null)
            {
                // Если событие не найдено, отклоняем бронь, так как она не может быть обработана без связанного события
                booking.Reject(DateTime.UtcNow);
                await appDbContext.SaveChangesAsync(stoppingToken);

                _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.Id);

                return;
            }

            // Подтверждаем бронь
            booking.Confirm(DateTime.UtcNow);
            await appDbContext.SaveChangesAsync(stoppingToken);

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

                if (@event is not null) @event.ReleaseSeats();

                await appDbContext.SaveChangesAsync(stoppingToken);
            }

            _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", bookingId);
        }
    }
}