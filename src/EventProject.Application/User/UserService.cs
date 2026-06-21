using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Abstractions.Services;
using EventProject.Application.User.DTOs;
using EventProject.Domain.Exceptions;

namespace EventProject.Application.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task RegisterUser(RegisterUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetUserByLogin(request.Login, ct);

        if (existingUser != null)
            throw new BadRequestException("Пользователь с указанным логином уже существует");

        var user = new Domain.Entities.User
        {
            UserId = Guid.NewGuid(),
            Login = request.Login,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await _userRepository.AddUser(user, ct);

        await _userRepository.SaveChanges(ct);
    }

    public async Task<string> LoginUser(LoginUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetUserByLogin(request.Login, ct);

        if (existingUser == null)
            throw new NotFoundException("Неверный логин или пароль!");

        var isValid = _passwordHasher.Verify(request.Password, existingUser.PasswordHash);

        return !isValid
            ? throw new NotFoundException("Неверный логин или пароль!")
            : _jwtTokenGenerator.GenerateToken(existingUser.UserId, existingUser.Login, existingUser.Role);
    }
}