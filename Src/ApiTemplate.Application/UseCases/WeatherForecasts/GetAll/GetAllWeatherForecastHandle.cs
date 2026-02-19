using ApiTemplate.Application.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

public class GetAllWeatherForecastHandle : IUseCaseHandle<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(GetAllWeatherForecastRequest request, CancellationToken cancellationToken = default)
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