namespace EventProject.Bookings.Application.Abstractions.Services;

public interface IBookingProducerService
{
    Task SendConfirm(Domain.Entities.Booking bookingConfirmed);
}