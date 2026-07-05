using EventProject.Users.Domain.Enums;

namespace EventProject.Users.Application.Abstractions.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, Role role);
    }
}
