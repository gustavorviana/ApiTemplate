using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data;

public class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
