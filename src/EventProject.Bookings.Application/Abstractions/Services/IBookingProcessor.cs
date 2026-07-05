namespace EventProject.Bookings.Application.Abstractions.Services;

public interface IBookingProcessor
{
    Task ProcessAsync(CancellationToken stoppingToken);
}