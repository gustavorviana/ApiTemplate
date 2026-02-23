using Viana.Results;
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
{
    private readonly IWeatherForecastRepository _repository;

    public GetAllWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var data = list.Select(e => new GetAllWeatherForecastResponse
        {
            Id = e.Id,
            Date = e.Date,
            TemperatureC = e.TemperatureC,
            Summary = e.Summary
        }).ToList();
        return new ListResult<GetAllWeatherForecastResponse>(data);
    }
}