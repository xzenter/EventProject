using EventProject.Application.Abstractions.Services;
using EventProject.Application.Booking;
using EventProject.Application.Event;
using EventProject.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingProcessor, BookingProcessor>();
    }
}