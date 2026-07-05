using EventProject.Users.Application.Abstractions.Services;
using EventProject.Users.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Users.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
    }
}