using ApiTemplate.Application.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Application.Interfaces;

public interface IDbContext
{
    DbSet<User> Users { get; }
    DbSet<WeatherForecast> WeatherForecasts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
