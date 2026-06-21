using EventProject.Domain.Enums;

namespace EventProject.Application.Abstractions.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string login, Role role);
    }
}
