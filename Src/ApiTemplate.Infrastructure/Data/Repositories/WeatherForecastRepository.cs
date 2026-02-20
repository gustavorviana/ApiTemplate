using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Infrastructure.Data.Repositories;

public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly IDbContext _context;

    public WeatherForecastRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<WeatherForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.WeatherForecasts.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.WeatherForecasts.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<WeatherForecast> AddAsync(WeatherForecast entity, CancellationToken cancellationToken = default)
    {
        await _context.WeatherForecasts.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(WeatherForecast entity, CancellationToken cancellationToken = default)
    {
        _context.WeatherForecasts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}