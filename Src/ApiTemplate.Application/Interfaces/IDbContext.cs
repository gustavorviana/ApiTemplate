using ApiTemplate.Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Application.Interfaces;

public interface IDbContext
{
#if (EnableJwtWithDatabase)
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
#endif
    DbSet<WeatherForecast> WeatherForecasts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
