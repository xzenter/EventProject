using EventProject.Auth.Application.Abstractions.Services;
using EventProject.Auth.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace EventProject.Auth.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
    }
}