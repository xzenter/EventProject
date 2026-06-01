using EventProject.Application.Abstractions.Repositories;
using EventProject.Domain.Entities;

namespace EventProject.Presentation.BackgroundServices;

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
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var pendingBookings = await bookingRepository.GetByStatus(BookingStatus.Pending, stoppingToken);

                var tasks = pendingBookings
                    .Take(50)
                    .Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));

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
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        // имитация внешнего вызова
        await Task.Delay(1000, stoppingToken);

        Domain.Entities.Event? @event = null;
        var booking = await bookingRepository.GetById(bookingId, stoppingToken);

        try
        {
            if (booking is null)
            {
                _logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            @event = await eventRepository.GetById(booking.EventId, stoppingToken);

            if (@event is null)
            {
                // Если событие не найдено, отклоняем бронь, так как она не может быть обработана без связанного события
                booking.Reject(DateTime.UtcNow);
                await bookingRepository.SaveChanges(stoppingToken);
                await eventRepository.SaveChanges(stoppingToken);

                _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.Id);

                return;
            }

            // Подтверждаем бронь
            booking.Confirm(DateTime.UtcNow);
            await bookingRepository.SaveChanges(stoppingToken);
            await eventRepository.SaveChanges(stoppingToken);

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

                await bookingRepository.SaveChanges(stoppingToken);
                await eventRepository.SaveChanges(stoppingToken);
            }

            _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", bookingId);
        }
    }
}