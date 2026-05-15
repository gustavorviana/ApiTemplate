using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastHandler(IDbContext db)
    : IUseCaseHandler<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
{
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
        CreateWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = WeatherForecast.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.TemperatureC,
            request.Summary);

        db.WeatherForecasts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateWeatherForecastResponse
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureC = entity.TemperatureC,
            Summary = entity.Summary
        };

        return new Result<CreateWeatherForecastResponse>(response, 201);
    }
}
