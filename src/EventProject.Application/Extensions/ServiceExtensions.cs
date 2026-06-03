using EventProject.Application.Abstractions.Services;
using EventProject.Application.BackgroundServices;
using EventProject.Application.Booking;
using EventProject.Application.Event;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddHostedService<BookingProcessingService>();
    }
}