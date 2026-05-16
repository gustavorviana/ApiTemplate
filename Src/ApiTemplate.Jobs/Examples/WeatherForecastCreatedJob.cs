using ApiTemplate.Contracts.Events;
using ExecutionFlow.Abstractions;

namespace ApiTemplate.Jobs.Examples;

public sealed class WeatherForecastCreatedJob : IHandler<WeatherForecastCreatedEvent>
{
    public Task HandleAsync(FlowContext<WeatherForecastCreatedEvent> context, CancellationToken ct)
    {
        var e = context.Event;
        context.Log.Info($"WeatherForecast {e.Id} created (Date={e.Date:O}, TempC={e.TemperatureC}, Summary={e.Summary ?? "<none>"})");
        return Task.CompletedTask;
    }
}
