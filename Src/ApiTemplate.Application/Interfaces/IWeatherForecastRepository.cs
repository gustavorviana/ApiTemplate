using ApiTemplate.Application.Core.Entities;

namespace ApiTemplate.Application.Interfaces;

public interface IWeatherForecastRepository
{
    Task<WeatherForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WeatherForecast> AddAsync(WeatherForecast entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(WeatherForecast entity, CancellationToken cancellationToken = default);
}
