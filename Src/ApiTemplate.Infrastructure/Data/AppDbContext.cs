using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    private const string EntitySuffix = "Entity";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public Guid? CurrentUserId { get; set; }

#if (EnableJwt)
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
#endif
    public DbSet<WeatherForecastEntity> WeatherForecasts => Set<WeatherForecastEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Strip the "Entity" suffix from table names so UserEntity -> User, etc.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var name = entityType.ClrType.Name;
            if (name.EndsWith(EntitySuffix))
                entityType.SetTableName(name[..^EntitySuffix.Length]);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    private void ApplyAuditFields()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (entry.Entity is AuditableEntity addedAudit && CurrentUserId is { } addedBy)
                        addedAudit.CreatedByUserId = addedBy;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // CreatedAt cannot be overwritten on update.
                    entry.Property(nameof(EntityBase.CreatedAt)).IsModified = false;

                    if (entry.Entity is AuditableEntity modifiedAudit)
                    {
                        // CreatedByUserId cannot be overwritten on update.
                        entry.Property(nameof(AuditableEntity.CreatedByUserId)).IsModified = false;
                        if (CurrentUserId is { } updatedBy)
                            modifiedAudit.UpdatedByUserId = updatedBy;
                    }
                    break;
            }
        }
    }
}
