using EventProject.Domain.Enums;

namespace EventProject.Application.User.DTOs;

public record RegisterUserRequest
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Роль пользователя, определяющая его права доступа.
    /// </summary>
    public Role Role { get; init; } = Role.User;
}