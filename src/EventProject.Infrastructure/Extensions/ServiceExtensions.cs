using System.Text;
using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Abstractions.Services;
using EventProject.Infrastructure.Auth;
using EventProject.Infrastructure.DataAccess;
using EventProject.Infrastructure.Repositories;
using EventProject.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EventProject.Infrastructure.Extensions;

public static class ServiceExtensions
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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}