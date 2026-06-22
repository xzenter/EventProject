using EventProject.Application.Abstractions.Services;
using EventProject.Application.Auth;
using EventProject.Application.Booking;
using EventProject.Application.Events;
using EventProject.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookingProcessor, BookingProcessor>();

        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));
    }
}