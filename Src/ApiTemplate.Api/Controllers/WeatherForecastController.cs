using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viana.Results;

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ListResult<GetAllWeatherForecastResponse>> Get(
        [FromServices] GetAllWeatherForecastHandle handle,
        CancellationToken token)
        => await handle.ExecuteAsync(new GetAllWeatherForecastRequest(), token);

#if (EnableJwt)
    [Authorize]
#endif
    [HttpPost(Name = "CreateWeatherForecast")]
    public async Task<IResult<CreateWeatherForecastResponse>> Create(
        [FromServices] CreateWeatherForecastHandle handle,
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken token)
        => await handle.ExecuteAsync(request, token);

#if (EnableJwt)
    [Authorize]
#endif
    [HttpDelete("{id:int}", Name = "DeleteWeatherForecast")]
    public async Task<Result> Delete(
        [FromServices] DeleteWeatherForecastHandle handle,
        [FromRoute] int id,
        CancellationToken token)
        => await handle.ExecuteAsync(new DeleteWeatherForecastRequest { Id = id }, token);
}
