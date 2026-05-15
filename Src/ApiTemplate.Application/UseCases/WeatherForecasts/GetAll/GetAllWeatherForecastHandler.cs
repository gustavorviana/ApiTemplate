using ApiTemplate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Viana.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;

public class GetAllWeatherForecastHandler(IDbContext db)
    : IUseCaseHandler<GetAllWeatherForecastRequest, ListResult<GetAllWeatherForecastResponse>>
{
    public async Task<ListResult<GetAllWeatherForecastResponse>> ExecuteAsync(
        GetAllWeatherForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await db.WeatherForecasts
            .AsNoTracking()
            .Select(e => new GetAllWeatherForecastResponse
            {
                Id = e.Id,
                Date = e.Date,
                TemperatureC = e.TemperatureC,
                Summary = e.Summary
            })
            .ToListAsync(cancellationToken);

        return new ListResult<GetAllWeatherForecastResponse>(data);
    }
}
