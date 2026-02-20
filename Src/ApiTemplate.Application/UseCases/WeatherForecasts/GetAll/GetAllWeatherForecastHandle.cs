#if (EnableResult)
using ApiTemplate.Application.Results;
#endif

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

#if (EnableResult)
public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
#else
public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, List<GetAllWeatherForecastResponse>>
#endif
{
    private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];
#if (EnableResult)
    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
#else
    public async Task<List<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
#endif
    {
        await Task.CompletedTask;
        return Enumerable.Range(1, 5).Select(index => new GetAllWeatherForecastResponse
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
    }
}