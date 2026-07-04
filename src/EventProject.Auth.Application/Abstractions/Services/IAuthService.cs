using EventProject.Auth.Application.Auth.DTOs;

namespace EventProject.Auth.Application.Abstractions.Services;

public interface IAuthService
{
    Task RegisterUser(RegisterUserRequest request, CancellationToken ct = default);

    Task<string> LoginUser(LoginUserRequest request, CancellationToken ct = default);
}