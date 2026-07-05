using EventProject.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Users.Infrastructure.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // Конструктор для EF Tools (без параметров)
    public AppDbContext() { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}