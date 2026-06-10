namespace EventProject.Application.Abstractions.Services;

public interface IBookingProcessor
{
    Task ProcessAsync(CancellationToken stoppingToken);
}