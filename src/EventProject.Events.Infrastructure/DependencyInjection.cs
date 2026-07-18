using System.Text;
using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Infrastructure.Auth;
using EventProject.Events.Infrastructure.BackgroundServices;
using EventProject.Events.Infrastructure.DataAccess;
using EventProject.Events.Infrastructure.Options;
using EventProject.Events.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace EventProject.Events.Infrastructure;

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

        services.AddScoped<IEventRepository, EventRepository>();

        // Получаем опции Kafka из конфигурации
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        services.AddHostedService<ConsumerWorker>();
        
        // Redis
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        var redisOptions = configuration
            .GetSection("Redis")
            .Get<RedisOptions>()!;

        var options = new ConfigurationOptions
        {
            Password = redisOptions.Password,
            ConnectTimeout = redisOptions.ConnectTimeout,
            SyncTimeout = redisOptions.SyncTimeout,
            AbortOnConnectFail = redisOptions.AbortOnConnectFail,
            ConnectRetry = redisOptions.ConnectRetry
        };

        options.EndPoints.Add(redisOptions.RedisServers);

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));
    }

    public static void DbMigrate(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}