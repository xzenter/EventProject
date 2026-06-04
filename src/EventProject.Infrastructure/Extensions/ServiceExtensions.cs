using EventProject.Application.Abstractions.Repositories;
using EventProject.Infrastructure.DataAccess;
using EventProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventProject.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static void AddInfrastuctureServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

#if DEBUG
            options
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors();
#endif
        });

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
    }
}