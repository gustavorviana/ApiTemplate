using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

public class GetAllWeatherForecastHandler(IAppDbContextFactory dbFactory)
    : IUseCaseHandler<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
{
    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(
        GetAllWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.Create();

        var data = await db.WeatherForecasts
            .AsNoTracking()
            .Select(e => new GetAllWeatherForecastResponse
            {
                Id = e.Id,
                Date = e.Date,
                TemperatureC = e.TemperatureC,
                Summary = e.Summary,
                CreatedAt = e.CreatedAt,
                CreatedByUserId = e.CreatedByUserId,
                UpdatedAt = e.UpdatedAt,
                UpdatedByUserId = e.UpdatedByUserId
            })
            .ToListAsync(cancellationToken);

        return new ListResult<GetAllWeatherForecastResponse>(data);
    }
}
