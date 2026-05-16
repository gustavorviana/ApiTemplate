namespace ApiTemplate.Contracts.Events;

public sealed record WeatherForecastCreatedEvent(Guid Id, DateOnly Date, int TemperatureC, string? Summary);
