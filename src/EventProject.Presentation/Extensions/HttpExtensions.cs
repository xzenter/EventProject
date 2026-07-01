using EventProject.Domain.Entities;

namespace EventProject.Presentation.Extensions;

public static class HttpExtensions
{
    public static User GetUser(this HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(nameof(User), out var value) && value is User user)
            return user;

        throw new InvalidOperationException("Пользователь не найден в контексте HTTP");
    }
}