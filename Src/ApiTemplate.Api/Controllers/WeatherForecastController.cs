using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using Microsoft.AspNetCore.Mvc;
using IResult = ApiTemplate.Application.Results.IResult;

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IResult> Get(
        [FromServices] GetAllWeatherForecastHandle handle,
        CancellationToken token)
        => await handle.ExecuteAsync(new GetAllWeatherForecastRequest(), token);

    [HttpPost(Name = "CreateWeatherForecast")]
    public async Task<IResult> Create(
        [FromServices] CreateWeatherForecastHandle handle,
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken token)
        => await handle.ExecuteAsync(request, token);
}