using EventProject.Models;
using EventProject.Repository.Booking;
using EventProject.Repository.Event;

namespace EventProject.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingProcessingService(IServiceProvider serviceProvider, ILogger<BookingProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис обработки брони запущен - {Time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var pendingBookings = repository
                    .GetByStatus(BookingStatus.Pending)
                    .ToList();

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

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

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        // имитация внешнего вызова
        await Task.Delay(1000, stoppingToken);

        await _processingSemaphore.WaitAsync(stoppingToken);

        try
        {
            Process(booking, stoppingToken, bookingRepository, eventRepository);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    private void Process(Booking booking, CancellationToken stoppingToken,
        IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        Event? @event = null;

        try
        {
            @event = eventRepository.GetById(booking.EventId);

            if (@event is null)
            {
                booking.Reject(DateTime.UtcNow);
                bookingRepository.Update(booking.Id, booking);

                _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.Id);
            }
            else
            {
                booking.Confirm(DateTime.UtcNow);
                bookingRepository.Update(booking.Id, booking);

                _logger.LogInformation("Бронь {BookingId} обработана и подтверждена", booking.Id);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            booking.Reject(DateTime.UtcNow);
            bookingRepository.Update(booking.Id, booking);

            if (@event is not null)
            {
                @event.ReleaseSeats();
                eventRepository.Update(@event.Id, @event);
            }

            _logger.LogInformation("Обработка брони {BookingId} была отменена", booking.Id);
        }
        catch (Exception)
        {
            booking.Reject(DateTime.UtcNow);
            bookingRepository.Update(booking.Id, booking);

            if (@event is not null)
            {
                @event.ReleaseSeats();
                eventRepository.Update(@event.Id, @event);
            }

            _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", booking.Id);
        }
    }
}