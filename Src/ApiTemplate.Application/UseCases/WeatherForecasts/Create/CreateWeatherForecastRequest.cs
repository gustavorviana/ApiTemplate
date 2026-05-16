namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastRequest
{
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}
