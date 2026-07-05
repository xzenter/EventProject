using System.Text;
using EventProject.Bookings.Application.Abstractions.Repositories;
using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Infrastructure.Auth;
using EventProject.Bookings.Infrastructure.DataAccess;
using EventProject.Bookings.Infrastructure.Repositories;
using EventProject.Bookings.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EventProject.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

#if DEBUG
            options
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors();
#endif
        });


        var jwtOptions = new JwtOptions
        {
            Secret = configuration["Jwt:Secret"]!,
            Issuer = configuration["Jwt:Issuer"]!,
            Audience = configuration["Jwt:Audience"]!,
            LifetimeMinutes = int.Parse(configuration["Jwt:LifetimeMinutes"]!)
        };

        services.AddSingleton(jwtOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddScoped<IBookingRepository, BookingRepository>();
    }

    public static void DbMigrate(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}