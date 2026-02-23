using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
{
    private readonly IWeatherForecastRepository _repository;

    public CreateWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
        CreateWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var entity = WeatherForecast.Create(date, request.TemperatureC, request.Summary);
        var added = await _repository.AddAsync(entity, cancellationToken);
        var response = new CreateWeatherForecastResponse
        {
            Id = added.Id,
            Date = added.Date,
            TemperatureC = added.TemperatureC,
            Summary = added.Summary
        };
        return new Result<CreateWeatherForecastResponse>(response, 201);
    }
}