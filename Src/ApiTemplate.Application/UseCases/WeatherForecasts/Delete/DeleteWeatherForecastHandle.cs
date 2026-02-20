#if (EnableResult)
using ApiTemplate.Application.Results;
#endif
using ApiTemplate.Application.Interfaces;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Delete;

#if (EnableResult)
public class DeleteWeatherForecastHandle : IUseCaseHandle<DeleteWeatherForecastRequest, Result>
#else
public class DeleteWeatherForecastHandle : IUseCaseHandle<DeleteWeatherForecastRequest, bool>
#endif
{
    private readonly IWeatherForecastRepository _repository;

    public DeleteWeatherForecastHandle(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

#if (EnableResult)
    public async Task<Result> ExecuteAsync(DeleteWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return new Result(404, new ProblemResult(404, "Weather forecast not found."));
        await _repository.DeleteAsync(entity, cancellationToken);
        return new Result(204);
    }
#else
    public async Task<bool> ExecuteAsync(DeleteWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return false;
        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }
#endif
}
