using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastHandler(IAppDbContextFactory dbFactory)
    : IUseCaseHandler<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
{
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
        CreateWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.Create();

        var entity = new WeatherForecastEntity
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = request.TemperatureC,
            Summary = request.Summary
        };

        db.WeatherForecasts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new Result<CreateWeatherForecastResponse>(new CreateWeatherForecastResponse
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureC = entity.TemperatureC,
            Summary = entity.Summary,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId
        }, 201);
    }
}
