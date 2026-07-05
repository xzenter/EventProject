using EventProject.Users.Application.Abstractions.Repositories;
using EventProject.Users.Application.Abstractions.Services;
using EventProject.Users.Application.Users.DTOs;
using EventProject.Users.Domain.Entities;
using EventProject.Users.Domain.Exceptions;

namespace EventProject.Users.Application.Auth;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task RegisterUser(RegisterUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await userRepository.GetUserByLogin(request.Login, ct);

        if (existingUser != null)
            throw new BadRequestException("Пользователь с указанным логином уже существует");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Login = request.Login,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await userRepository.AddUser(user, ct);
        await userRepository.SaveChanges(ct);
    }

    public async Task<string> LoginUser(LoginUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await userRepository.GetUserByLogin(request.Login, ct);

        if (existingUser == null)
            throw new NotFoundException("Неверный логин или пароль");

        var isValid = passwordHasher.Verify(request.Password, existingUser.PasswordHash);

        return isValid
            ? jwtTokenGenerator.GenerateToken(existingUser.UserId, existingUser.Login, existingUser.Role)
            : throw new NotFoundException("Неверный логин или пароль");
    }
}