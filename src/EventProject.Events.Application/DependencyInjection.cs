using EventProject.Events.Application.Abstractions.Services;
using EventProject.Events.Application.Caching;
using EventProject.Events.Application.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Events.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventService, EventService>();

        services.Configure<CacheTtlOptions>(configuration.GetSection("Redis:Ttl"));
    }
}