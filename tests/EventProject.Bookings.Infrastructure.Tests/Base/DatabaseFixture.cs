using EventProject.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventProject.Bookings.Infrastructure.Tests.Base;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public DatabaseFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine").Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public AppDbContext CreateContext()
    {
        var connectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}