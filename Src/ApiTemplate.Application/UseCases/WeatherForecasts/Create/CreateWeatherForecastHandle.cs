using ApiTemplate.Application.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastHandle : IUseCaseHandle<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
{
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
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

        return new Result<CreateWeatherForecastResponse>(201, response);
    }
}