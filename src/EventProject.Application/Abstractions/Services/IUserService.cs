using EventProject.Application.User.DTOs;

namespace EventProject.Application.Abstractions.Services;

public interface IUserService
{
    Task RegisterUser(RegisterUserRequest request, CancellationToken ct = default);

    Task<string> LoginUser(LoginUserRequest request, CancellationToken ct = default);
}