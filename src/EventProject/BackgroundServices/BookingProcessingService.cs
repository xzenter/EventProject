using EventProject.Models;
using EventProject.Repository.Booking;

namespace EventProject.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingService> _logger;

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
                await ProcessBookingsAsync(stoppingToken);
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

    private async Task ProcessBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var bookings = repository
            .GetByStatus(BookingStatus.Pending)
            .Where(b => b is { Status: BookingStatus.Pending })
            .ToList();

        foreach (var booking in bookings)
        {
            // Имитация обработки
            await Task.Delay(2000, stoppingToken);

            repository.Update(
                booking.Id,
                new Booking
                {
                    Id = booking.Id,
                    EventId = booking.EventId,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = booking.CreatedAt,
                    ProcessedAt = DateTime.UtcNow
                });
        }
    }
}