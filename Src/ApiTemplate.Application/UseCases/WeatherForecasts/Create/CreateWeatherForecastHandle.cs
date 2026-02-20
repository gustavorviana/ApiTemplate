#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

#if (EnableResult)
public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
#else
public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, CreateWeatherForecastResponse>
#endif
{
    private readonly IWeatherForecastRepository _repository;

    public CreateWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

#if (EnableResult)
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
#else
    public async Task<CreateWeatherForecastResponse> ExecuteAsync(
#endif
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
#if (EnableResult)
        return new Result<CreateWeatherForecastResponse>(201, response);
#else
        return response;
#endif
    }
}