#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

#if (EnableResult)
public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
#else
public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, List<GetAllWeatherForecastResponse>>
#endif
{
    private readonly IWeatherForecastRepository _repository;

    public GetAllWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

#if (EnableResult)
    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
#else
    public async Task<List<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
#endif
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var data = list.Select(e => new GetAllWeatherForecastResponse
        {
            Id = e.Id,
            Date = e.Date,
            TemperatureC = e.TemperatureC,
            Summary = e.Summary
        }).ToList();
#if (EnableResult)
        return new ListResult<GetAllWeatherForecastResponse>(data);
#else
        return data;
#endif
    }
}