using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Infrastructure.Fakes;

public class FakeWeatherForecastRepository : IWeatherForecastRepository
{
    private static readonly List<WeatherForecast> Store = [];
    private static int _nextId = 1;
    private static readonly object _lock = new();

    public Task<WeatherForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var entity = Store.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(entity);
        }
    }

    public Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var list = Store.ToList();
            return Task.FromResult<IReadOnlyList<WeatherForecast>>(list);
        }
    }

    public Task<WeatherForecast> AddAsync(WeatherForecast entity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var id = _nextId++;
            var added = WeatherForecast.Create(entity.Date, entity.TemperatureC, entity.Summary);
            added.Id = id;
            Store.Add(added);
            return Task.FromResult(added);
        }
    }

    public Task DeleteAsync(WeatherForecast entity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            Store.RemoveAll(e => e.Id == entity.Id);
            return Task.CompletedTask;
        }
    }
}