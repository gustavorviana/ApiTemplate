using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Delete;

public class DeleteWeatherForecastHandler(IDbContext db)
    : IUseCaseHandler<DeleteWeatherForecastRequest, Result>
{
    public async Task<Result> ExecuteAsync(
        DeleteWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.WeatherForecasts
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
            return new Result(404, new ProblemResult(404, Messages.WeatherForecast.WeatherForecastNotFound));

        db.WeatherForecasts.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new Result(204);
    }
}
