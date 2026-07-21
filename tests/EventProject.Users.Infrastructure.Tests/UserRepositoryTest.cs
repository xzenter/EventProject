using EventProject.Users.Domain.Entities;
using EventProject.Users.Domain.Enums;
using EventProject.Users.Infrastructure.Repositories;
using EventProject.Users.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Users.Tests;

[Collection("Database collection")]
public class UserRepositoryTest
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = _fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE users RESTART IDENTITY CASCADE");
    }

    [Fact]
    public async Task AddUser_SavesUserToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var repository = new UserRepository(context);
        var user = CreateUser("AddUser_SavesUser", Role.User);

        // Act
        await repository.AddUser(user, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var saved = await verifyContext.Users
            .FirstOrDefaultAsync(u => u.UserId == user.UserId, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal("AddUser_SavesUser", saved.Login);
        Assert.Equal(user.PasswordHash, saved.PasswordHash);
        Assert.Equal(Role.User, saved.Role);
    }

    [Fact]
    public async Task AddUser_AdminUser_SavesAdminToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var repository = new UserRepository(context);
        var user = CreateUser("AddUser_AdminUser", Role.Admin);

        // Act
        await repository.AddUser(user, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var saved = await verifyContext.Users
            .FirstOrDefaultAsync(u => u.UserId == user.UserId, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(Role.Admin, saved.Role);
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var user = CreateUser("GetUser_ExistingUser", Role.User);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUser(user.UserId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal("GetUser_ExistingUser", result.Login);
        Assert.Equal(Role.User, result.Role);
    }

    [Fact]
    public async Task GetUser_UnknownUser_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUser(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByLogin_ExistingUser_ReturnsUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();

        var user = CreateUser("GetUserByLogin_ExistingUser", Role.User);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUserByLogin("GetUserByLogin_ExistingUser", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal("GetUserByLogin_ExistingUser", result.Login);
    }

    [Fact]
    public async Task GetUserByLogin_UnknownLogin_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUserByLogin("nonexistent_login", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChanges_WithNoChanges_ReturnsZero()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.SaveChanges(CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    private static User CreateUser(string login, Role role)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            Login = login,
            PasswordHash = "hashed_password",
            Role = role
        };
    }
}