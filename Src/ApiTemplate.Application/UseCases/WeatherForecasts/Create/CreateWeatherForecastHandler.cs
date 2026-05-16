using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
#if (EnableHangfire)
using ApiTemplate.Contracts.Events;
using ExecutionFlow.Abstractions;
#endif
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastHandler(
    IAppDbContextFactory dbFactory
#if (EnableHangfire)
    , IEventDispatcher dispatcher
#endif
) : IUseCaseHandler<CreateWeatherForecastRequest, Result<CreateWeatherForecastResponse>>
{
    public async Task<Result<CreateWeatherForecastResponse>> ExecuteAsync(
        CreateWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.Create();

        var entity = new WeatherForecastEntity
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = request.TemperatureC,
            Summary = request.Summary
        };

        db.WeatherForecasts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

#if (EnableHangfire)
        dispatcher.Publish(new WeatherForecastCreatedEvent(entity.Id, entity.Date, entity.TemperatureC, entity.Summary));
#endif

        return new Result<CreateWeatherForecastResponse>(new CreateWeatherForecastResponse
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureC = entity.TemperatureC,
            Summary = entity.Summary,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId
        }, 201);
    }
}
