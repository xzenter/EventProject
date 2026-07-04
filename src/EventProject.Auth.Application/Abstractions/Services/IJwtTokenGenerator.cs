using EventProject.Auth.Domain.Enums;

namespace EventProject.Auth.Application.Abstractions.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, Role role);
    }
}
