using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.MessagesCatalog;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Delete;

public class DeleteWeatherForecastHandle : IUseCaseHandle<DeleteWeatherForecastRequest, Result>
{
    private readonly IWeatherForecastRepository _repository;

    public DeleteWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ExecuteAsync(DeleteWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return new Result(404, new ProblemResult(404, Messages.WeatherForecast.WeatherForecastNotFound));
        await _repository.DeleteAsync(entity, cancellationToken);
        return new Result(204);
    }
}
