using System.Security.Claims;
using EventProject.Application.Abstractions.Repositories;
using EventProject.Domain.Entities;

namespace EventProject.Presentation.Middlewares;

/// <summary>
/// Middleware для записи пользователя в контекст запроса после удачной авторизации.
/// </summary>
public class ExecuteUserMiddleware
{
    private readonly RequestDelegate _next;

    public ExecuteUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx, IUserRepository userRepository)
    {
        if (ctx.User.Identity?.IsAuthenticated ?? false)
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userRepository.GetUser(Guid.Parse(userId), CancellationToken.None);
                ctx.Items[nameof(User)] = user;
            }
        }

        await _next(ctx);
    }
}