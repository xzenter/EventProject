using EventProject.Users.Application.Abstractions.Repositories;
using EventProject.Users.Application.Abstractions.Services;
using EventProject.Users.Application.Auth;
using EventProject.Users.Application.Auth.DTOs;
using EventProject.Users.Domain.Entities;
using EventProject.Users.Domain.Enums;
using EventProject.Users.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace EventProject.Users.Application.Tests;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object
        );
    }

    [Fact]
    public async Task RegisterUser_WithUniqueLogin_CreatesUserAndSaves()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Login = "new_user",
            Password = "password123",
            Role = Role.User
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed_password");

        // Act
        await _authService.RegisterUser(request, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUser(
            It.Is<User>(u => u.Login == request.Login
                           && u.PasswordHash == "hashed_password"
                           && u.Role == Role.User),
            It.IsAny<CancellationToken>()), Times.Once);

        _userRepositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateLogin_ThrowsBadRequestException()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Login = "existing_user",
            Password = "password123",
            Role = Role.User
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                UserId = Guid.NewGuid(),
                Login = request.Login,
                PasswordHash = "existing_hash",
                Role = Role.User
            });

        // Act
        var act = () => _authService.RegisterUser(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Пользователь с указанным логином уже существует");

        _userRepositoryMock.Verify(x => x.AddUser(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUser_WithAdminRole_CreatesAdminUser()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Login = "admin_user",
            Password = "admin_pass",
            Role = Role.Admin
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed_admin_pass");

        // Act
        await _authService.RegisterUser(request, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUser(
            It.Is<User>(u => u.Role == Role.Admin),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Login = "valid_user",
            Password = "correct_password"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Login = request.Login,
            PasswordHash = "hashed_password",
            Role = Role.User
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, request.Login, Role.User))
            .Returns("jwt_token_value");

        // Act
        var token = await _authService.LoginUser(request, CancellationToken.None);

        // Assert
        token.Should().Be("jwt_token_value");

        _jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(userId, request.Login, Role.User), Times.Once);
    }

    [Fact]
    public async Task LoginUser_WithNonExistentLogin_ThrowsNotFoundException()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Login = "nonexistent",
            Password = "any_password"
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _authService.LoginUser(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Неверный логин или пароль");

        _jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task LoginUser_WithInvalidPassword_ThrowsNotFoundException()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Login = "valid_user",
            Password = "wrong_password"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Login = request.Login,
            PasswordHash = "correct_hash",
            Role = Role.User
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByLogin(request.Login, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _authService.LoginUser(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Неверный логин или пароль");

        _jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()), Times.Never);
    }
}