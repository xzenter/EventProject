using EventProject.Domain.Entities;

namespace EventProject.Presentation.Extensions;

public static class HttpExtensions
{
    public static bool TryGetUser(this HttpContext ctx, out User record)
    {
        var result = ctx.Items.TryGetValue(nameof(User), out var property);
        record = property as User;
        return result;
    }

    public static User GetUser(this HttpContext ctx)
    {
        if (ctx.TryGetUser(out var user)) return user;
        throw new InvalidOperationException("Пользователь не найден в контексте HTTP");
    }
}