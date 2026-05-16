using ApiTemplate.Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Application.Interfaces;

public interface IAppDbContext : IAsyncDisposable, IDisposable
{
    Guid? CurrentUserId { get; set; }

#if (EnableJwt)
    DbSet<UserEntity> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
#endif
    DbSet<WeatherForecastEntity> WeatherForecasts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
