using EventProject.Application.Abstractions.Services;
using EventProject.Application.Booking;
using EventProject.Application.Events;
using EventProject.Application.Services;
using EventProject.Application.Settings;
using EventProject.Application.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookingProcessor, BookingProcessor>();

        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));
    }
}