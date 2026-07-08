using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Application.Services;

namespace EventProject.Bookings.Presentation.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessingService> _logger;

    public BookingProcessingService(IServiceScopeFactory scopeFactory, ILogger<BookingProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис обработки брони запущен - {Time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<BookingConfirmProcessor>();

            await processor.Execute(stoppingToken);

            await Task.Delay(10000, stoppingToken);
        }

        _logger.LogInformation("Сервис управления обработкой брони остановлен");
    }
}