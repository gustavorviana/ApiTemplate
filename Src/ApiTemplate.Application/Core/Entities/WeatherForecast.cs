namespace ApiTemplate.Application.Core.Entities;

public class WeatherForecast
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }

    private WeatherForecast() { }

    public static WeatherForecast Create(DateOnly date, int temperatureC, string? summary)
    {
        return new WeatherForecast
        {
            Date = date,
            TemperatureC = temperatureC,
            Summary = summary
        };
    }
}
