using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Application.Booking;
using EventProject.Bookings.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Bookings.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingProcessor, BookingProcessor>();

        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));
    }
}