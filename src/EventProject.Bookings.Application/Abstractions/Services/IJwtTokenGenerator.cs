using EventProject.Bookings.Domain.Enums;

namespace EventProject.Bookings.Application.Abstractions.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string login, Role role);
}