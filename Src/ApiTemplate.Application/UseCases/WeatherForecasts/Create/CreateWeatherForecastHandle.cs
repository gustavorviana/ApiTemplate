#if (EnableResult)
using ApiTemplate.Application.Results;
#endif

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

#if (EnableResult)
public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
#else
public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, CreateWeatherForecastResponse>
#endif
{

#if (EnableResult)
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
#else
    public async Task<CreateWeatherForecastResponse> ExecuteAsync(

#endif
    CreateWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var response = new CreateWeatherForecastResponse
        {
            Id = Random.Shared.Next(1, 10_000),
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = request.TemperatureC,
            Summary = request.Summary
        };

#if (EnableResult)
        return new Result<CreateWeatherForecastResponse>(201, response);
#else
        return response;
#endif

    }
}