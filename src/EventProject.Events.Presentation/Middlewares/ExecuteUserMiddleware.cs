using System.Security.Claims;
using EventProject.Events.Domain.Entities;
using EventProject.Events.Domain.Enums;

namespace EventProject.Events.Presentation.Middlewares;

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

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated ?? false)
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userLogin = ctx.User.FindFirstValue(ClaimTypes.Name);
            var role = ctx.User.FindFirstValue(ClaimTypes.Role);
            var result = Enum.TryParse<Role>(role, true, out var userRole);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userLogin))
            {
                var user = new User
                {
                    UserId = Guid.Parse(userId),
                    Login = userLogin,
                    Role = result ? Role.User : userRole
                };

                ctx.Items[nameof(User)] = user;
            }
        }

        await _next(ctx);
    }
}