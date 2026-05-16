namespace ApiTemplate.Domain.Entities;

public class WeatherForecastEntity : AuditableEntity
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}
